using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace Catalogo2026
{
    public partial class LoginWindow : Window
    {
        // Práctica 3: los usuarios ya no están en memoria, se validan contra
        // la tabla Usuarios de MySQL (ver DatabaseHelper y database.sql).

        const int MaxIntentos = 3;
        const int SegundosBloqueo = 30;

        // Código de error de MySQL cuando se viola una restricción UNIQUE
        const int ErrorClaveDuplicada = 1062;

        // Intentos fallidos de cada usuario
        Dictionary<string, int> intentosUsuario = new Dictionary<string, int>();

        // Momento hasta el que cada usuario permanece bloqueado
        Dictionary<string, DateTime> bloqueadoHasta = new Dictionary<string, DateTime>();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void MostrarError(string mensaje)
        {
            lblMensaje.Foreground = Brushes.Red;
            lblMensaje.Text = mensaje;
        }

        private void MostrarExito(string mensaje)
        {
            lblMensaje.Foreground = Brushes.Green;
            lblMensaje.Text = mensaje;
        }

        // Comprueba que los dos campos están rellenos (TC 01). Si falta alguno,
        // muestra el aviso y no se llega a consultar la base de datos.
        private bool CamposRellenos(out string user, out string pass)
        {
            user = txtUsuario.Text.Trim();
            pass = txtPassword.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MostrarError("Introduzca usuario y contraseña.");
                return false;
            }
            return true;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // TC 01: campos vacíos -> no se consulta la BD
            if (!CamposRellenos(out string user, out string pass))
                return;

            // ¿El usuario sigue bloqueado por intentos fallidos?
            if (bloqueadoHasta.TryGetValue(user, out DateTime fin))
            {
                if (DateTime.Now < fin)
                {
                    int restantes = (int)Math.Ceiling((fin - DateTime.Now).TotalSeconds);
                    MostrarError($"El usuario está bloqueado temporalmente. Inténtelo de nuevo en {restantes} s.");
                    return;
                }
                bloqueadoHasta.Remove(user);
                intentosUsuario[user] = 0;
            }

            bool loginCorrecto;

            try
            {
                using MySqlConnection conexion = DatabaseHelper.AbrirConexion();

                // Consulta PARAMETRIZADA: los datos del usuario nunca se concatenan
                // en el texto SQL, así que ' OR '1'='1 no funciona (TC 09).
                const string sql =
                    "SELECT COUNT(*) FROM Usuarios " +
                    "WHERE nombre_usuario = @usuario AND password = @password";

                using MySqlCommand comando = new MySqlCommand(sql, conexion);
                comando.Parameters.AddWithValue("@usuario", user);
                comando.Parameters.AddWithValue("@password", pass);

                long coincidencias = Convert.ToInt64(comando.ExecuteScalar());
                loginCorrecto = coincidencias == 1;
            }
            catch (MySqlException)
            {
                // TC 08: MySQL parado o inaccesible -> aviso y la app sigue abierta
                MostrarError("No se puede conectar con la base de datos. Compruebe que MySQL está en marcha e inténtelo de nuevo.");
                return;
            }

            if (!loginCorrecto)
            {
                // TC 04: mismo mensaje tanto si falla el usuario como la contraseña,
                // para no dar pistas a un atacante sobre qué usuarios existen.
                intentosUsuario[user] = intentosUsuario.GetValueOrDefault(user) + 1;
                txtPassword.Clear();

                if (intentosUsuario[user] >= MaxIntentos)
                {
                    bloqueadoHasta[user] = DateTime.Now.AddSeconds(SegundosBloqueo);
                    MostrarError($"Usuario o contraseña incorrectos. Usuario bloqueado temporalmente {SegundosBloqueo} segundos.");
                }
                else
                {
                    MostrarError($"Usuario o contraseña incorrectos. (Intento {intentosUsuario[user]} de {MaxIntentos})");
                }
                return;
            }

            // TC 03 / TC 07: inicio de sesión correcto -> se abre la Home
            intentosUsuario[user] = 0;

            MainWindow home = new MainWindow(user);
            home.Show();
            this.Close();
        }

        // TC 05 / TC 06: alta de un usuario nuevo en la tabla Usuarios
        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (!CamposRellenos(out string user, out string pass))
                return;

            try
            {
                using MySqlConnection conexion = DatabaseHelper.AbrirConexion();

                const string sql =
                    "INSERT INTO Usuarios (nombre_usuario, password) " +
                    "VALUES (@usuario, @password)";

                using MySqlCommand comando = new MySqlCommand(sql, conexion);
                comando.Parameters.AddWithValue("@usuario", user);
                comando.Parameters.AddWithValue("@password", pass);
                comando.ExecuteNonQuery();

                txtPassword.Clear();
                MostrarExito($"Usuario «{user}» dado de alta correctamente. Ya puede iniciar sesión.");
            }
            catch (MySqlException ex) when (ex.Number == ErrorClaveDuplicada)
            {
                // TC 06: nombre_usuario es UNIQUE, MySQL rechaza el duplicado
                MostrarError("El usuario ya existe.");
            }
            catch (MySqlException)
            {
                // TC 08: fallo de conexión
                MostrarError("No se puede conectar con la base de datos. Compruebe que MySQL está en marcha e inténtelo de nuevo.");
            }
        }

        // TC 02: Salir de la aplicación
        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
