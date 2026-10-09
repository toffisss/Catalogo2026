using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Catalogo2026
{
    public partial class LoginWindow : Window
    {
        // Matriz bidimensional en memoria simulando la base de datos (Usuario, Contraseña)
        string[,] usuarios = new string[,] { { "admin", "1234" }, { "alumno", "dam2026" } };

        // Diccionario para contar los intentos fallidos de cada usuario
        Dictionary<string, int> intentosUsuario = new Dictionary<string, int>();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text.Trim();
            string pass = txtPassword.Password.Trim();

            // TC04: No se rellenan los campos
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMensaje.Foreground = Brushes.Red;
                lblMensaje.Text = "Introduzca usuario y contraseña.";
                return;
            }

            // Validar si el usuario está bloqueado por intentos previos (TC05 / TC07)
            if (intentosUsuario.ContainsKey(user) && intentosUsuario[user] >= 3)
            {
                lblMensaje.Foreground = Brushes.Red;
                lblMensaje.Text = "El usuario está bloqueado temporalmente por exceso de intentos.";
                return;
            }

            bool usuarioExiste = false;
            bool loginExitoso = false;

            // Recorrer la matriz para buscar coincidencias
            for (int i = 0; i < usuarios.GetLength(0); i++)
            {
                if (usuarios[i, 0] == user)
                {
                    usuarioExiste = true;

                    if (usuarios[i, 1] == pass)
                    {
                        loginExitoso = true;
                    }
                    break;
                }
            }

            // TC03: Inicio de sesión si usuario no existe
            if (!usuarioExiste)
            {
                lblMensaje.Foreground = Brushes.Red;
                lblMensaje.Text = "El usuario introducido no existe.";
                return;
            }

            // TC02: Inicio de sesión fallido (contraseña incorrecta)
            if (!loginExitoso)
            {
                // Registrar intento fallido
                if (intentosUsuario.ContainsKey(user))
                    intentosUsuario[user]++;
                else
                    intentosUsuario.Add(user, 1);

                lblMensaje.Foreground = Brushes.Red;
                lblMensaje.Text = "La contraseña introducida no es correcta.";
            }
            else
            {
                // TC01: Inicio de sesión exitoso
                lblMensaje.Foreground = Brushes.Green;
                lblMensaje.Text = "Acceso concedido.";

                // Reiniciar intentos
                if (intentosUsuario.ContainsKey(user))
                    intentosUsuario[user] = 0;

                // Redirigir al formulario principal (F_Principal / MainWindow)
                MainWindow home = new MainWindow();
                home.Show();
                this.Close(); // Cerramos la ventana de Login
            }
        }

        // TC06: Salir de la aplicación
        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}