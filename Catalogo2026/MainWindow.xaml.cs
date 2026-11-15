using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Catalogo2026
{
    public partial class MainWindow : Window
    {
        private readonly string usuario;
        private int articulosCarrito = 0;

        private readonly List<Juego> misJuegos = new List<Juego>();
        private readonly ObservableCollection<Publicacion> publicaciones = new ObservableCollection<Publicacion>();

        // Carrusel de la portada
        private readonly List<string> imagenesCarrusel = new List<string>();
        private readonly DispatcherTimer temporizadorCarrusel = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        private readonly Random azar = new Random();
        private int imagenActual = -1;

        // Paneles de contenido y botones de navegación, por nombre de sección
        private readonly Dictionary<string, FrameworkElement> paneles;
        private readonly Dictionary<string, Button> botonesNav;

        // Constructor sin parámetros (lo usa el diseñador de Visual Studio)
        public MainWindow() : this("invitado") { }

        // Constructor que recibe el usuario que ha iniciado sesión
        public MainWindow(string usuario)
        {
            InitializeComponent();
            this.usuario = usuario;

            paneles = new Dictionary<string, FrameworkElement>
            {
                ["Destacados"] = panelDestacados,
                ["MisJuegos"] = panelMisJuegos,
                ["Configuracion"] = panelConfiguracion,
                ["Tienda"] = panelTienda,
                ["Comunidad"] = panelComunidad,
                ["Soporte"] = panelSoporte,
                ["Perfil"] = panelPerfil
            };

            botonesNav = new Dictionary<string, Button>
            {
                ["Destacados"] = btnDestacados,
                ["MisJuegos"] = btnMisJuegos,
                ["Configuracion"] = btnConfiguracion,
                ["Tienda"] = btnTienda,
                ["Comunidad"] = btnComunidad,
                ["Soporte"] = btnSoporte,
                ["Perfil"] = btnPerfil
            };

            btnPerfil.Content = $"👤 {usuario}";
            CargarDatos();
            CargarPerfil();
            chkTemaOscuro.IsChecked = true;   // tema oscuro por defecto

            MostrarSeccion("Destacados");
            txtEstado.Text = $"Bienvenido, {usuario}.";

            IniciarCarrusel();
            Closed += (s, e) => temporizadorCarrusel.Stop();   // al cerrar la ventana, se para
        }

        // ================= CARRUSEL =================

        // Busca TODAS las imágenes que hay en la carpeta Imagenes del proyecto
        // (las que están incluidas como "Resource"). Si añades más, salen solas.
        private void IniciarCarrusel()
        {
            var ensamblado = Assembly.GetExecutingAssembly();
            var recursos = new ResourceManager(ensamblado.GetName().Name + ".g", ensamblado);
            var conjunto = recursos.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            if (conjunto != null)
            {
                foreach (DictionaryEntry recurso in conjunto)
                {
                    string ruta = recurso.Key.ToString() ?? "";          // p. ej. "imagenes/juego1.jpg"
                    string extension = Path.GetExtension(ruta);
                    if (ruta.StartsWith("imagenes/") &&
                        (extension == ".jpg" || extension == ".jpeg" || extension == ".png"))
                    {
                        imagenesCarrusel.Add(ruta);
                    }
                }
            }

            if (imagenesCarrusel.Count == 0) return;

            MostrarImagen(azar.Next(imagenesCarrusel.Count));
            temporizadorCarrusel.Tick += (s, e) => MostrarImagenAleatoria();
            temporizadorCarrusel.Start();
        }

        private void MostrarImagenAleatoria()
        {
            if (imagenesCarrusel.Count < 2) return;
            int nueva;
            do { nueva = azar.Next(imagenesCarrusel.Count); } while (nueva == imagenActual);   // nunca repite la misma
            MostrarImagen(nueva);
        }

        private void MostrarImagen(int indice)
        {
            imagenActual = indice;
            string ruta = imagenesCarrusel[indice];

            imgCarrusel.Source = new BitmapImage(new Uri($"pack://application:,,,/{ruta}"));
            txtCarruselTitulo.Text = TituloDeImagen(ruta);
            txtCarruselPosicion.Text = $"{indice + 1} / {imagenesCarrusel.Count}";

            // Pequeña animación de aparición (fundido)
            imgCarrusel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(600)));
        }

        // Títulos "bonitos" de las imágenes conocidas (nombre de archivo -> título)
        private static readonly Dictionary<string, string> TitulosConocidos = new Dictionary<string, string>
        {
            ["juego1"] = "Anime Fighters",
            ["juego2"] = "Minecraft",
            ["juego3"] = "Super Mario Bros. Wonder",
            ["hollow_knight"] = "Hollow Knight",
            ["stardew_valley"] = "Stardew Valley",
            ["hades_ii"] = "Hades II",
            ["elden_ring"] = "Elden Ring",
            ["baldurs_gate_3"] = "Baldur's Gate 3",
            ["cyberpunk_2077"] = "Cyberpunk 2077",
            ["it_takes_two"] = "It Takes Two",
            ["rocket_league"] = "Rocket League",
            ["celeste"] = "Celeste",
            ["portal_2"] = "Portal 2"
        };

        // Título a mostrar según el nombre del archivo
        private static string TituloDeImagen(string ruta)
        {
            string nombre = Path.GetFileNameWithoutExtension(ruta);
            if (TitulosConocidos.TryGetValue(nombre, out string? titulo))
                return titulo;

            // Si no está en la lista: "mi_juego_nuevo" -> "Mi Juego Nuevo"
            var palabras = nombre.Replace('-', '_').Split('_', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => char.ToUpper(p[0]) + p.Substring(1));
            return string.Join(" ", palabras);
        }

        private void CarruselAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (imagenesCarrusel.Count == 0) return;
            MostrarImagen((imagenActual - 1 + imagenesCarrusel.Count) % imagenesCarrusel.Count);
            temporizadorCarrusel.Stop(); temporizadorCarrusel.Start();   // reinicia la cuenta de 4 s
        }

        private void CarruselSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (imagenesCarrusel.Count == 0) return;
            MostrarImagen((imagenActual + 1) % imagenesCarrusel.Count);
            temporizadorCarrusel.Stop(); temporizadorCarrusel.Start();
        }

        // ================= DATOS DE EJEMPLO =================
        // (títulos y precios de ejemplo para la maqueta)

        private static ImageSource Portada(string archivo) =>
            new BitmapImage(new Uri($"pack://application:,,,/Imagenes/{archivo}"));

        private static Brush Pincel(string hex) =>
            (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;

        private void CargarDatos()
        {
            icDestacados.ItemsSource = new List<Juego>
            {
                new Juego("Anime Fighters", "Lucha", "29,99 €", "#C0392B", "Ver ficha", Portada("juego1.jpg")),
                new Juego("Minecraft", "Sandbox · Supervivencia", "26,95 €", "#27AE60", "Ver ficha", Portada("juego2.jpg")),
                new Juego("Super Mario Bros. Wonder", "Plataformas", "59,99 €", "#E67E22", "Ver ficha", Portada("juego3.jpg")),
                new Juego("Hollow Knight", "Metroidvania", "14,99 €", "#34495E", "Ver ficha"),
                new Juego("Stardew Valley", "Simulación · Granja", "13,99 €", "#16A085", "Ver ficha")
            };

            misJuegos.AddRange(new[]
            {
                new Juego("Minecraft", "Sandbox", "", "#27AE60", "", Portada("juego2.jpg")) { Horas = "124 h jugadas" },
                new Juego("The Legend of Zelda: Tears of the Kingdom", "Aventura", "", "#2980B9", "") { Horas = "87 h jugadas" },
                new Juego("Rocket League", "Deportes", "", "#8E44AD", "") { Horas = "45 h jugadas" },
                new Juego("Celeste", "Plataformas", "", "#D35400", "") { Horas = "12 h jugadas" },
                new Juego("Portal 2", "Puzles", "", "#7F8C8D", "") { Horas = "9 h jugadas" }
            });
            icMisJuegos.ItemsSource = misJuegos;
            txtResumenBiblioteca.Text = $"Tienes {misJuegos.Count} juegos en tu biblioteca.";

            icTienda.ItemsSource = new List<Juego>
            {
                new Juego("Hades II", "Roguelike", "29,50 €", "#922B21", "Añadir"),
                new Juego("Elden Ring", "Rol · Acción", "59,99 €", "#7D6608", "Añadir"),
                new Juego("Baldur's Gate 3", "Rol", "59,99 €", "#4A235A", "Añadir"),
                new Juego("Cyberpunk 2077", "Rol · Acción", "59,99 €", "#B7950B", "Añadir"),
                new Juego("It Takes Two", "Cooperativo", "39,99 €", "#1F618D", "Añadir"),
                new Juego("Hollow Knight: Silksong", "Metroidvania", "19,50 €", "#5D6D7E", "Añadir")
            };

            publicaciones.Add(new Publicacion("profesor", "Recordad: la Práctica 3 se entrega el domingo 22 a las 23:59.", "hace 1 h"));
            publicaciones.Add(new Publicacion("alumno", "¿Alguien para una partida de Rocket League esta noche?", "hace 3 h"));
            publicaciones.Add(new Publicacion("admin", "Este fin de semana hay torneo de la comunidad. ¡Apuntaos!", "ayer"));
            icPublicaciones.ItemsSource = publicaciones;
        }

        private void CargarPerfil()
        {
            txtPerfilNombre.Text = usuario;
            txtPerfilInicial.Text = usuario.Length > 0 ? usuario.Substring(0, 1).ToUpper() : "?";
            var es = new CultureInfo("es-ES");
            txtPerfilMiembro.Text = $"Miembro desde {DateTime.Now.ToString("MMMM 'de' yyyy", es)}";
            txtStatJuegos.Text = misJuegos.Count.ToString();
            txtStatHoras.Text = misJuegos.Sum(j => int.Parse(j.Horas.Split(' ')[0])).ToString();
        }

        // ================= NAVEGACIÓN =================

        // Todos los botones de menú llaman aquí; su Tag dice qué sección abrir
        private void Navegar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag is string seccion)
                MostrarSeccion(seccion);
        }

        private void MostrarSeccion(string seccion)
        {
            foreach (var panel in paneles.Values)
                panel.Visibility = Visibility.Collapsed;
            paneles[seccion].Visibility = Visibility.Visible;

            // Resalta el botón de la sección activa
            foreach (var boton in botonesNav.Values)
                boton.ClearValue(BackgroundProperty);
            botonesNav[seccion].Background = (Brush)FindResource("HoverBrush");
        }

        // ================= ACCIONES =================

        // Botón de las tarjetas: "Ver ficha" en Destacados, "Añadir" en la Tienda
        private void TarjetaBoton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Juego juego }) return;

            if (juego.TextoBoton == "Añadir")
            {
                articulosCarrito++;
                txtCarrito.Text = $"🛒 {articulosCarrito}";
                txtEstado.Text = $"«{juego.Titulo}» añadido al carrito ({articulosCarrito} artículos).";
            }
            else
            {
                MessageBox.Show($"{juego.Titulo}\n\nGénero: {juego.Genero}\nPrecio: {juego.Precio}",
                                "Ficha del juego", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Jugar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Juego juego })
                txtEstado.Text = $"▶ Iniciando «{juego.Titulo}»…";
        }

        private void TemaOscuro_Changed(object sender, RoutedEventArgs e)
        {
            bool oscuro = chkTemaOscuro.IsChecked == true;
            Resources["FondoBrush"]      = Pincel(oscuro ? "#171A21" : "#F3F4F6");
            Resources["PanelBrush"]      = Pincel(oscuro ? "#1B2838" : "#FFFFFF");
            Resources["TarjetaBrush"]    = Pincel(oscuro ? "#22344A" : "#E9EDF2");
            Resources["TextoBrush"]      = Pincel(oscuro ? "#E6EDF3" : "#111827");
            Resources["TextoSuaveBrush"] = Pincel(oscuro ? "#8F98A0" : "#6B7280");
            Resources["BordeBrush"]      = Pincel(oscuro ? "#2A3F5A" : "#D1D5DB");
            Resources["AcentoBrush"]     = Pincel(oscuro ? "#1A9FFF" : "#2563EB");
            Resources["HoverBrush"]      = Pincel(oscuro ? "#2A475E" : "#DCE3EC");

            // Vuelve a resaltar la sección activa con el color del nuevo tema
            var activa = paneles?.FirstOrDefault(p => p.Value.Visibility == Visibility.Visible).Key;
            if (activa != null) MostrarSeccion(activa);
        }

        private void GuardarConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            string idioma = (cbIdioma.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Español";
            string avisos = chkNotificaciones.IsChecked == true ? "sí" : "no";
            txtEstado.Text = $"✔ Configuración guardada · Idioma: {idioma} · Notificaciones: {avisos} · Volumen: {sldVolumen.Value:0} %";
        }

        private void Publicar_Click(object sender, RoutedEventArgs e)
        {
            string texto = txtNuevaPublicacion.Text.Trim();
            if (texto.Length == 0)
            {
                txtEstado.Text = "Escribe algo antes de publicar.";
                return;
            }
            publicaciones.Insert(0, new Publicacion(usuario, texto, "ahora mismo"));
            txtNuevaPublicacion.Clear();
            txtEstado.Text = "Publicación compartida con la comunidad.";
        }

        private void EnviarSoporte_Click(object sender, RoutedEventArgs e)
        {
            if (cbAsunto.SelectedIndex < 0 || txtMensajeSoporte.Text.Trim().Length == 0)
            {
                txtSoporteResultado.Foreground = Brushes.IndianRed;
                txtSoporteResultado.Text = "Elige un asunto y escribe tu mensaje.";
                return;
            }
            int ticket = new Random().Next(1000, 9999);
            txtSoporteResultado.Foreground = Brushes.MediumSeaGreen;
            txtSoporteResultado.Text = $"Consulta enviada. Nº de ticket: #{ticket}";
            txtMensajeSoporte.Clear();
            cbAsunto.SelectedIndex = -1;
        }

        private void EditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("La edición del perfil estará disponible próximamente.",
                            "Mi perfil", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Cerrar sesión: vuelve al login (prueba de regresión TC 02)
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }

    // Un juego del catálogo
    public class Juego
    {
        public string Titulo { get; }
        public string Genero { get; }
        public string Precio { get; }
        public Brush ColorFondo { get; }
        public string TextoBoton { get; }
        public ImageSource? Imagen { get; }
        public string Horas { get; set; } = "0 h jugadas";
        public string Inicial => Titulo.Substring(0, 1);

        public Juego(string titulo, string genero, string precio, string colorHex,
                     string textoBoton, ImageSource? imagen = null)
        {
            Titulo = titulo;
            Genero = genero;
            Precio = precio;
            ColorFondo = (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex)!;
            TextoBoton = textoBoton;
            Imagen = imagen;
        }
    }

    // Una publicación de la comunidad
    public class Publicacion
    {
        public string Autor { get; }
        public string Texto { get; }
        public string Hace { get; }

        public Publicacion(string autor, string texto, string hace)
        {
            Autor = autor;
            Texto = texto;
            Hace = hace;
        }
    }
}
