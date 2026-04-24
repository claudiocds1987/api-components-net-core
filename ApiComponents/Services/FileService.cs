namespace ApiComponents.Services
{
    // Helper simple para inyectar o usar en el Service/Repo
    public class FileService
    {
        private readonly string _uploadFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public async Task<string> ProcessImage(string imageData, string scheme, string host)
        {
            // 1. Si ya es una URL, no se procesa
            if (imageData.StartsWith("http")) return imageData;

            // 2.  // Si es Base64: "data:image/png;base64,iVBO..." importada desde mi pc
            if (imageData.StartsWith("data:image"))
            {
                // Validar formato básico
                if (!imageData.Contains(",")) throw new Exception("Formato de imagen Base64 inválido.");

                // Decodificar
                var base64Data = imageData.Split(',')[1];
                var bytes = Convert.FromBase64String(base64Data);

                // --- VALIDACIÓN SENIOR DE TAMAÑO (2MB) ---
                if (bytes.Length > 2 * 1024 * 1024)
                {
                    throw new Exception("El archivo es demasiado grande. El máximo permitido es 2MB.");
                }

                // Asegurar que la carpeta exista
                if (!Directory.Exists(_uploadFolder)) Directory.CreateDirectory(_uploadFolder);

                // Generar nombre único
                var fileName = $"prod_{Guid.NewGuid()}.png";
                var filePath = System.IO.Path.Combine(_uploadFolder, fileName);

                // Guardar archivo físico
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                // Retornar URL para el frontend
                return $"{scheme}://{host}/uploads/{fileName}";
            }

            return imageData;
        }
    }
}
