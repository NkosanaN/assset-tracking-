using Application.Contracts.Persistence;
using Application.UserPhotos;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Photos;

public class PhotoAccessor : IPhotoAccessor
{
	private readonly Cloudinary _cloudinary;
	public PhotoAccessor(IOptions<CloudinarySettings> config)
	{
		var acc = new Account(
				config.Value.CloudName,
				config.Value.ApiKey,
				config.Value.ApiSecret
		);

		_cloudinary = new Cloudinary(acc);
	}
	public async Task<PhotoUploadResult> AddPhoto(IFormFile file)
	{
		if (file.Length > 0)
		{
			await using var stream = file.OpenReadStream();

			var uploadParams = new ImageUploadParams
			{
				File = new FileDescription(file.FileName, stream),
				Transformation = new Transformation().Height(500).Width(500).Crop("fill")
			};

			var uploadResult = await _cloudinary.UploadAsync(uploadParams);

			if (uploadResult.Error != null)
			{
				throw new Exception(uploadResult.Error.Message);
			}

			return new PhotoUploadResult
			{
				PublicId = uploadResult.PublicId,
				Url = uploadResult.SecureUrl.ToString()
			};
		}

		return null!;
	}

	public async Task<string> DeletePhoto(string publicId)
	{
		var deleteParam = new DeletionParams(publicId);

		var result = await _cloudinary.DestroyAsync(deleteParam);

		return result.Result == "ok" ? result.Result : null;

	}
}
