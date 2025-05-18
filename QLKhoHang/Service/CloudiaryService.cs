using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace QLKhoHang.Service
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }
      
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            
            if (file == null || file.Length == 0)
                return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"users/image/",
                PublicId = $"{Guid.NewGuid()}",
                Transformation = new Transformation().Width(500).Height(500).Crop("limit")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.AbsoluteUri;
        }

        public async Task<List<string>> UploadMutilImage(List<IFormFile> files)
        {
            
            var imageUrls = new List<string>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = $"auction/users/images/",
                    PublicId = $"{Guid.NewGuid()}",
                    Transformation = new Transformation().Width(500).Height(500).Crop("limit")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                imageUrls.Add(uploadResult.SecureUrl.AbsoluteUri);
            }

            return imageUrls;
        }

        public async Task<string> UploadImageAvatarAsync(IFormFile file)
        {
            
            if (file == null || file.Length == 0)
                return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"users/avatar/",
                PublicId = $"{Guid.NewGuid()}",
                Transformation = new Transformation().Width(500).Height(500).Crop("limit")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.AbsoluteUri;
        }
        public async Task DeleteImageAsync(string imageUrl)
        {
            var publicId = GetPublicIdFromUrl(imageUrl);
            var deletionParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deletionParams);
        }

        private static string GetPublicIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var segments = uri.Segments;
            var publicId = segments[segments.Length - 1];
            return publicId.Split('.')[0];
        }
    }
}
