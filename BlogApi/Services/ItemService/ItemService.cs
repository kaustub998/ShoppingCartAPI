using EcorpAPI.Models;
using Microsoft.EntityFrameworkCore;
using XSystem;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EcorpAPI.Services.ItemService
{
    public class ItemService : IItemService
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public ItemService(ShoppingCartContext shoppingCartContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _shoppingCartContext = shoppingCartContext;
        }

        private async Task<List<DetailedShoppingItem>> GetDetailedItemData(bool isAdmin = false, int itemId = 0)
        {
            var data = (from shoppingItem in _shoppingCartContext.ShoppingItems
                        join user in _shoppingCartContext.UserDetail on shoppingItem.UserId equals user.UserId
                        where shoppingItem.IsDeleted != true
                        && (shoppingItem.UserId == CommonService.GetUserId(_httpContextAccessor.HttpContext) && isAdmin != true || isAdmin)
                        && (itemId > 0 && shoppingItem.ItemId == itemId || itemId == 0)
                        select new DetailedShoppingItem
                        {
                            ItemId = shoppingItem.ItemId,
                            ItemName = shoppingItem.ItemName,
                            ItemDescription = shoppingItem.ItemDescription,
                            CreatedOn = shoppingItem.CreatedOn,
                            User_FullName = user.FirstName + " " + user.LastName,
                            UserId = shoppingItem.UserId,
                        }).ToList();

            return data;
        }

        private async Task<List<DetailedShoppingItem>> GetItemImage(List<DetailedShoppingItem> shoppingItems)
        {
            var shoppingItemIds = shoppingItems.Select(itemm => itemm.ItemId).ToList();

            var images = await _shoppingCartContext.ItemImages
                    .Where(img => shoppingItemIds.Contains(img.ItemId ?? -1) && img.IsDeleted != true)
                    .ToListAsync();

            foreach (var shoppingItem in shoppingItems)
            {
                List<ItemImageDetailed> detailedImages = images.Select(img => new ItemImageDetailed
                {
                    ImageId = img.ImageId,
                    ItemId = img.ItemId,
                    ImagePath = img.ImagePath,
                }).ToList();

                foreach (var img in detailedImages)
                {
                    img.ImageBytes = await ReadLocalImageAsByteArray(img.ImagePath);
                };

                shoppingItem.ItemImages = detailedImages;
            }

            return shoppingItems;
        }
        private async Task<byte[]> ReadLocalImageAsByteArray(string imagePath)
        {
            try
            {

                byte[] imageData = await File.ReadAllBytesAsync(imagePath);
                return imageData;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during reading
                Console.WriteLine($"Error reading image file: {ex.Message}");
                return null;
            }
        }
        private async Task<string[]> SaveImagesAsync(int itemId, List<byte[]> images)
        {
            var _imageFolderPath = _configuration["ImagePath"];
            List<string> imageUrls = new List<string>();

            // Create a directory for the shoppingitem if it doesn't exist
            string itemFolderPath = Path.Combine(_imageFolderPath, $"item{itemId}");
            Directory.CreateDirectory(itemFolderPath);

            // Save each image
            for (int i = 0; i < images.Count; i++)
            {
                string imagePath = Path.Combine(itemFolderPath, $"{itemId}_image_{i}.jpg");
                await File.WriteAllBytesAsync(imagePath, images[i]);
                imageUrls.Add(imagePath);
            }

            return imageUrls.ToArray();
        }

        public async Task<List<DetailedShoppingItem>> GetItemList(bool isShopPage = true)
        {
            bool isAdmin = false;
            try
            {
                isAdmin = await _shoppingCartContext.UserDetail.Where(item => item.UserId == CommonService.GetUserId(_httpContextAccessor.HttpContext)).Select(item => item.IsAdmin).FirstOrDefaultAsync();
            }
            catch (Exception ex) { }

            List<DetailedShoppingItem> data = await GetDetailedItemData(isAdmin || isShopPage);

            data = await GetItemImage(data);

            return data.ToList();
        }

        public async Task<DetailedShoppingItem> GetItemDetail(int? id)
        {
            var data = await GetDetailedItemData(true, id ?? 0);
            data = await GetItemImage(data);
            return data.FirstOrDefault() ?? new DetailedShoppingItem();
        }

        public async Task<ResponseModel> AddItemAsync(AddEditShoppingItem shoppingItem)
        {
            ResponseModel response = new ResponseModel();

            ShoppingItem shoppingitem = new ShoppingItem
            {
                UserId = shoppingItem.UserId,
                ItemName = shoppingItem.ItemName?.Trim(),
                ItemDescription = shoppingItem.ItemDescription?.Trim(),
                CreatedOn = DateTime.Now,
                CreatedBy = CommonService.GetUserId(_httpContextAccessor.HttpContext),
            };

            try
            {
                _shoppingCartContext.ShoppingItems.Add(shoppingitem);
                await _shoppingCartContext.SaveChangesAsync();

                response.isError = false;
                response.isSuccess = true;
                response.message = "Item Added Successfully!!!";

                List<string> imageUrls = new List<string>();

                var _imageFolderPath = _configuration["ImagePath"];

                if (shoppingItem.ItemImages != null)
                {
                    foreach (var image in shoppingItem.ItemImages)
                    {
                        string imageName = $"{shoppingitem.ItemId}_image_{Guid.NewGuid()}.jpg";
                        string imagePath = Path.Combine(_imageFolderPath, imageName);

                        try
                        {
                            await File.WriteAllBytesAsync(imagePath, image.ImageBytes);
                            imageUrls.Add(imagePath);
                        }
                        catch (Exception ex)
                        {
                            // Handle or log the exception
                            Console.WriteLine($"Error saving image: {ex.Message}");
                        }
                    }

                    // Create Image entities for database insertion
                    List<ItemImage> itemImages = imageUrls.Select(url => new ItemImage
                    {
                        ItemId = shoppingitem.ItemId,
                        ImagePath = url
                    }).ToList();

                    try
                    {
                        // Add Image entities to the database
                        _shoppingCartContext.ItemImages.AddRange(itemImages);
                        await _shoppingCartContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Handle or log the exception
                        Console.WriteLine($"Error saving ItemImages to the database: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                response.isError = true;
                response.isSuccess = false;
                response.message = "Something Went Wrong!!!";
            }

            return response;
        }

        public async Task<ResponseModel> EditItemAsync(AddEditShoppingItem shoppingItem)
        {
            ResponseModel response = new ResponseModel();
            var _imageFolderPath = _configuration["ImagePath"];

            try
            {
                ShoppingItem? editItem = await _shoppingCartContext.ShoppingItems.Where(item => item.ItemId == shoppingItem.ItemId && item.IsDeleted != true).FirstOrDefaultAsync();

                if (editItem != null)
                {
                    editItem.ItemName = shoppingItem.ItemName;
                    editItem.ItemDescription = shoppingItem.ItemDescription;
                    editItem.ModifiedOn = DateTime.Now;
                    editItem.ModifiedBy = CommonService.GetUserId(_httpContextAccessor.HttpContext);

                    _shoppingCartContext.ShoppingItems.Update(editItem);
                    await _shoppingCartContext.SaveChangesAsync();

                    response.isError = false;
                    response.isSuccess = true;
                    response.message = "Item Edited Successfully!!!";

                    List<ItemImage> existingImages = await _shoppingCartContext.ItemImages.Where(item => item.IsDeleted != true && item.ItemId == shoppingItem.ItemId).ToListAsync();
                    foreach (var existingImage in existingImages.ToList())
                    {
                        // Check if the existing image is not present in the new images
                        if (!shoppingItem.ItemImages.Any(newImage => newImage.ImageId == existingImage.ImageId))
                        {
                            // Mark the image as deleted in the database
                            existingImage.IsDeleted = true;
                            _shoppingCartContext.ItemImages.Update(existingImage);
                        }
                    }

                    // Add new images to the database
                    foreach (var newImage in shoppingItem.ItemImages)
                    {
                        // Check if the new image is not present in the existing images
                        if (!existingImages.Any(existingImage => existingImage.ImageId == newImage.ImageId))
                        {
                            string imageName = $"{shoppingItem.ItemId}_image_{Guid.NewGuid()}.jpg";
                            string imagePath = Path.Combine(_imageFolderPath, imageName);
                            await File.WriteAllBytesAsync(imagePath, newImage.ImageBytes);

                            // Add the new image to the database
                            _shoppingCartContext.ItemImages.Add(new ItemImage
                            {
                                ItemId = shoppingItem.ItemId,
                                ImagePath = imagePath,
                            });
                        }
                    }
                    await _shoppingCartContext.SaveChangesAsync();
                }
                else
                {
                    response.isError = true;
                    response.isSuccess = false;
                    response.message = "Item Not Found!!!";
                }
            }
            catch (Exception ex)
            {
                response.isError = true;
                response.isSuccess = false;
                response.message = "Something Went Wrong!!!";
            }

            return response;
        }

        public async Task<ResponseModel> DeleteItemAsync(int? shoppingItemId)
        {
            ResponseModel response = new ResponseModel();

            try
            {
                ShoppingItem? shoppingItem = await _shoppingCartContext.ShoppingItems.Where(x => x.ItemId == shoppingItemId && x.IsDeleted != true).FirstOrDefaultAsync();
                if (shoppingItem != null)
                {
                    shoppingItem.IsDeleted = true;
                    shoppingItem.ModifiedBy = CommonService.GetUserId(_httpContextAccessor.HttpContext);
                    shoppingItem.ModifiedOn = DateTime.UtcNow;

                    _shoppingCartContext.ShoppingItems.Update(shoppingItem);
                    await _shoppingCartContext.SaveChangesAsync();

                    response.isError = false;
                    response.isSuccess = true;
                    response.message = "Item Deleted Successfully!!!";
                }
                else
                {
                    response.isError = false;
                    response.isSuccess = true;
                    response.message = "Item Has Already Been Deleted!!!";
                }
            }
            catch
            {
                response.isError = false;
                response.isSuccess = true;
                response.message = "Something Went Wrong!!!";
            }

            return response;
        }

    }
}
