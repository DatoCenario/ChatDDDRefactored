using System.Text.RegularExpressions;
using Chatiks.Core.Data.EF;
using Chatiks.Core.Data.EF.Domain;
using Chatiks.Core.Domain;
using Chatiks.Core.Specifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Image = Chatiks.Core.Data.EF.Domain.Image;

namespace Chatiks.Core.Managers;

public class ImagesManager
{
    private readonly CoreRepository _coreRepository;
    private readonly ImageDomainModelFactory _imageDomainModelFactory;
    
    public ImagesManager(CoreRepository coreRepository, ImageDomainModelFactory imageDomainModelFactory)
    {
        _coreRepository = coreRepository;
        _imageDomainModelFactory = imageDomainModelFactory;
    }
    
    public async Task<List<ImageDomainModel>> UploadNewImagesAsync(ICollection<string> base64texts)
    {
        var images= base64texts.Select(x => _imageDomainModelFactory.CreateNew(x)).ToList();

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