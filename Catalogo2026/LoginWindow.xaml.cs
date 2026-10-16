using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Catalogo2026
{
    public partial class LoginWindow : Window
    {
        // Matriz bidimensional en memoria simulando la base de datos (Usuario, Contraseña)
        string[,] usuarios = new string[,] { { "admin", "1234" }, { "alumno", "dam2026" } };

        const int MaxIntentos = 3;
        const int SegundosBloqueo = 30;

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

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Password.Trim();

            // TC04: No se rellenan los campos
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MostrarError("Introduzca usuario y contraseña.");
                return;
            }

            // TC05: ¿el usuario sigue bloqueado?
            if (bloqueadoHasta.TryGetValue(user, out DateTime fin))
            {
                if (DateTime.Now < fin)
                {
                    int restantes = (int)Math.Ceiling((fin - DateTime.Now).TotalSeconds);
                    MostrarError($"El usuario está bloqueado temporalmente. Inténtelo de nuevo en {restantes} s.");
                    return;
                }

                // El bloqueo ya expiró: se reinician los intentos
                bloqueadoHasta.Remove(user);
                intentosUsuario[user] = 0;
            }

            bool usuarioExiste = false;
            bool loginExitoso = false;

            // Recorrer la matriz para buscar coincidencias
            for (int i = 0; i < usuarios.GetLength(0); i++)
            {
                if (usuarios[i, 0] == user)
                {
                    usuarioExiste = true;
                    loginExitoso = usuarios[i, 1] == pass;
                    break;
                }
            }

            // TC03: Usuario no existe
            if (!usuarioExiste)
            {
                MostrarError("El usuario introducido no existe.");
                return;
            }

            // TC02 / TC05: Contraseña incorrecta
            if (!loginExitoso)
            {
                intentosUsuario[user] = intentosUsuario.GetValueOrDefault(user) + 1;
                txtPassword.Clear();

                if (intentosUsuario[user] >= MaxIntentos)
                {
                    bloqueadoHasta[user] = DateTime.Now.AddSeconds(SegundosBloqueo);
                    MostrarError($"La contraseña introducida no es correcta. Usuario bloqueado temporalmente {SegundosBloqueo} segundos.");
                }
                else
                {
                    MostrarError($"La contraseña introducida no es correcta. (Intento {intentosUsuario[user]} de {MaxIntentos})");
                }
                return;
            }

            // TC01: Inicio de sesión exitoso
            intentosUsuario[user] = 0;

            MainWindow home = new MainWindow();
            home.Show();
            this.Close();
        }

        // TC07: Salir de la aplicación
        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}