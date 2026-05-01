using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Data;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;

namespace SmartCenter.Service.Cart;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    
    private Guid GetStudentId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
                        .FindFirst("studentId")?.Value;

        if (claim == null)
            throw new UnauthorizedAccessException("Not found information of student");

        return Guid.Parse(claim);
    }
    
    public async Task CreateCart(Guid studentId)
    {
        var existCart =  await _dbContext.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.StuId == studentId);
        
        if (existCart != null)
        {
            throw new Exception("Cart exists");
        }
        var cart = new Repository.Entity.Cart()
        {
            StuId = studentId,
        };
        
        _dbContext.Carts.Add(cart);
        await  _dbContext.SaveChangesAsync();
    }

    public async Task<List<Response.CartItemResponse?>> GetCartItem(Guid studentId)
    {
        var items = await _dbContext.CartItems
            .Where(x => x.Cart.StuId == studentId)
            .Include(x => x.Course)
            .Include(x => x.Combo)
                .ThenInclude(c => c!.ComboCourses)
                    .ThenInclude(cc => cc.Course)
            .ToListAsync();

        return items.Select<CartItem, Response.CartItemResponse?>(item =>
        {
            if (item.ItemType == CartItemType.Course && item.Course != null)
            {
                return new Response.CartItemResponse()
                {
                    ItemName = item.Course.CourseName,
                    ItemDescription = item.Course.Description,
                    ItemType = CartItemType.Course,
                    ItemPrice = item.Course.BasePrice,
                    Quantity = item.Quantity,
                    ItemAmount = item.Quantity * item.Course.BasePrice
                };
            }

            if (item.ItemType == CartItemType.Combo && item.Combo != null)
            {
                var totalBase      = item.Combo.ComboCourses.Sum(cc => cc.Course.BasePrice);
                var discountedPrice = totalBase * (1 - item.Combo.DiscountPercent / 100m);
                return new Response.CartItemResponse
                {
                    ItemName        = item.Combo.Name,
                    ItemDescription = $"Combo {item.Combo.ComboCourses.Count} course, discount {item.Combo.DiscountPercent}%",
                    ItemPrice       = discountedPrice,
                    ItemAmount      = discountedPrice * item.Quantity,
                    ItemType        = CartItemType.Combo,
                    Quantity        = item.Quantity,
                };
            }
            return null;
        }).ToList();
    }

    public async Task AddItemToCart(Request.AddItemToCartRequest request)
    {

        var studentId = GetStudentId();
        
        var cart = await _dbContext.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.StuId == studentId);
        if (cart == null)
            throw new Exception("Cart not found");

        CartItem newItem;
        if (request.ItemType == CartItemType.Course)
        {
            var course = await _dbContext.Courses
                .FirstOrDefaultAsync(x => x.Id == request.ItemId && x.IsActive);
            if (course != null)
                throw new Exception("Course not found or inactive");
            
            
            if(cart.Items.Any(x => x.CourseId == request.ItemId))
                throw new Exception("Course already exists");
            
            newItem = new CartItem
            {
                CartId   = cart.Id,
                CourseId = course!.Id,
                ComboId  = null,
                ItemType = CartItemType.Course,
                Quantity = 1,
            };
        }
        else if (request.ItemType == CartItemType.Combo)
        {
            var combo = await _dbContext.Combos
                .FirstOrDefaultAsync(x => x.Id == request.ItemId && x.IsActive);
            if (combo != null)
                throw new Exception("Combo not found or inactive");
            if (cart.Items.Any(x => x.ComboId == request.ItemId))
                throw new Exception("Combo already exists");
            newItem = new CartItem
            {
                CartId   = cart.Id,
                ComboId  = combo!.Id,
                CourseId = null,
                ItemType = CartItemType.Combo,
                Quantity = 1,
            };
        }
        else
        {
            throw new Exception("Invalid item type");
        }
        
        _dbContext.CartItems.Add(newItem);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveItemFromCart(Request.RemoveItemFromCartRequest request)
    {
        
        var studentId = GetStudentId();
        
        var cartItem = await _dbContext.CartItems
            .FirstOrDefaultAsync(x => x.Id == request.ItemId && x.Cart.StuId == studentId);
        if (cartItem == null)
            throw new Exception("Cart not found");
        
        _dbContext.CartItems.Remove(cartItem);
        await _dbContext.SaveChangesAsync();
    }
}