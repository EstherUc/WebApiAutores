namespace WebApiAutores.Servicios
{
    public class EscribirEnArchivo : IHostedService
    {
        private readonly IWebHostEnvironment env;
        private readonly string nombreArchivo = "Archivo 1.txt";
        private Timer timer;

        public EscribirEnArchivo(IWebHostEnvironment env) //IWebHostEnvironment env, me va a permitir acceder al ambiente en el cual yo me encuentre
        {
            this.env = env;
        }
        public Task StartAsync(CancellationToken cancellationToken)//Se ejecuta cuando cargemos nuestra webApi
        {
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));//cada 5 segundos se va a ejecutar el timer
            Escribir("Proceso iniciado");
            return Task.CompletedTask; //para decir que esta tarea ha acabado
        }

        public Task StopAsync(CancellationToken cancellationToken)//Se ejecuta cuando apagamos nuestro webApi
        {
            timer.Dispose();//para detener el timer
            Escribir("Proceso finalizado");
            return Task.CompletedTask; //para decir que esta tarea ha acabado
        }

        private void DoWork(object state)
        {
            Escribir("Proceso en ejecución: " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
        }

        //método auxiliar para escribir en el archivo
        private void Escribir(string mensaje)
        {
            var ruta = $@"{env.ContentRootPath}\wwwroot\{nombreArchivo}";
            using (StreamWriter writer = new StreamWriter(ruta, append: true))
            {
                writer.WriteLine(mensaje);    
            }
        }
    }
}
