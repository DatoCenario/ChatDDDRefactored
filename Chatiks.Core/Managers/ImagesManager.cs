using System.Text.RegularExpressions;
using Chatiks.Core.Data.EF;
using Chatiks.Core.Data.EF.Domain;
using Chatiks.Core.Specifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Image = Chatiks.Core.Data.EF.Domain.Image;

namespace Chatiks.Core.Managers;

public class ImagesManager
{
    private readonly int _maxImageBytes = 12000;
    private readonly Regex _replaceImageHeaderReg = new Regex(@"^data:image\/(png|jpg);base64,");
    
    private readonly CoreRepository _coreRepository;
    
    public ImagesManager(CoreRepository coreRepository)
    {
        _coreRepository = coreRepository;
    }

    public async Task<long> UploadNewImageAsync(string base64text)
    {
        var imageWithoutHeader = _replaceImageHeaderReg.Replace(base64text, "");
        var imageBytes = Convert.FromBase64String(base64text);
        if (imageBytes.Length > _maxImageBytes)
        {
            // no need to load my server (not quite correct algorithm - optimizes size by delta but not bytes lenght)
            using (var image = SixLabors.ImageSharp.Image.Load(imageBytes, new PngDecoder()))
            {
                var delta = Math.Sqrt(imageBytes.Length / _maxImageBytes);
                image.Mutate(o => o.Resize(new Size
                {
                    Width = (int)(image.Width / delta),
                    Height = (int)(image.Height / delta)
                }));
                imageWithoutHeader = image.ToBase64String(PngFormat.Instance);
                imageWithoutHeader = _replaceImageHeaderReg.Replace(imageWithoutHeader, "");
            }
        }
        
        using (var isolatedOperation = _coreRepository.BeginIsolatedOperation())
        {
            var imageExisting = await _coreRepository.LoadImageBySpecificationAsync(new ImagesSpecification(new ImagesFilter()
            {
                Base64Text = base64text
            }));

            if (imageExisting == null)
            {
                var entry = await _coreRepository.AddImageAsync(imageWithoutHeader);
                imageExisting = entry.Entity;
            }
            
            await _coreRepository.SaveChangesAsync();
            return imageExisting.Id;
        }
    }
    
    public async Task<List<long>> UploadNewImagesAsync(ICollection<string> base64texts)
    {
        var images= new List<Image>();
        var imagesToAdd = new List<string>();

        foreach (var base64text in base64texts)
        {
            var imageWithoutHeader = _replaceImageHeaderReg.Replace(base64text, "");
            var imageBytes = Convert.FromBase64String(base64text);
            if (imageBytes.Length > _maxImageBytes)
            {
                using (var image = SixLabors.ImageSharp.Image.Load(imageBytes, new PngDecoder()))
                {
                    var delta = Math.Sqrt(imageBytes.Length / _maxImageBytes);
                    image.Mutate(o => o.Resize(new Size
                    {
                        Width = (int)(image.Width / delta),
                        Height = (int)(image.Height / delta)
                    }));
                    imageWithoutHeader = image.ToBase64String(PngFormat.Instance);
                    imageWithoutHeader = _replaceImageHeaderReg.Replace(imageWithoutHeader, "");
                }
            }

            imagesToAdd.Add(imageWithoutHeader);
        }

        using (var isolatedOperation = _coreRepository.BeginIsolatedOperation())
        {
            foreach (var image in imagesToAdd)
            {
                // Optimize
                var imageExisting = await _coreRepository.LoadImageBySpecificationAsync(new ImagesSpecification(new ImagesFilter()
                {
                    Base64Text = image
                }));

                if (imageExisting == null)
                {
                    var entry = await _coreRepository.AddImageAsync(image);
                    images.Add(entry.Entity);
                }
            }

            await _coreRepository.SaveChangesAsync();
        }

        return images.Select(x => x.Id).ToList();
    }
    
    public Task<Image> GetImage(ImagesSpecification specification)
    {
        using (var io = _coreRepository.BeginIsolatedOperation())
        {
            return _coreRepository.LoadImageBySpecificationAsync(specification);
        }
    }

    public Task<ICollection<Image>> GetImages(ImagesSpecification specification)
    {
        using (var io = _coreRepository.BeginIsolatedOperation())
        {
            return _coreRepository.LoadImagesBySpecificationAsync(specification);
        }
    }
}