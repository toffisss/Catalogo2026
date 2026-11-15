using MySql.Data.MySqlClient;

namespace Catalogo2026
{
    /// <summary>
    /// Centraliza el acceso a MySQL: guarda la cadena de conexión
    /// y se encarga de abrir y cerrar las conexiones.
    /// </summary>
    public static class DatabaseHelper
    {
        // Cadena de conexión. Cambia Password si usas otro usuario de MySQL.
        // Connection Timeout corto para que, si MySQL está parado (TC 08),
        // el aviso aparezca en unos segundos y no a los 15 s por defecto.
        private const string CadenaConexion =
            "Server=localhost;Port=3306;Database=catalogo2026;" +
            "User ID=catalogo_app;Password=Catalogo2026!;" +
            "Connection Timeout=5;";

        /// <summary>
        /// Crea y abre una conexión. Lanza MySqlException si MySQL no responde.
        /// Úsala siempre dentro de un bloque using para que se cierre sola.
        /// </summary>
        public static MySqlConnection AbrirConexion()
        {
            var conexion = new MySqlConnection(CadenaConexion);
            conexion.Open();
            return conexion;
        }

        /// <summary>
        /// Cierra la conexión si está abierta.
        /// </summary>
        public static void CerrarConexion(MySqlConnection? conexion)
        {
            if (conexion != null && conexion.State != System.Data.ConnectionState.Closed)
            {
                conexion.Close();
            }
        }
    }
}
