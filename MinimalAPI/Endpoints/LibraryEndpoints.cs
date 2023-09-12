using FluentValidation;
using FluentValidation.Results;
using MinimalAPI.Data;
using MinimalAPI.Endpoints.Internal;
using MinimalAPI.Services;

namespace MinimalAPI.Endpoints;

public class LibraryEndpoints : IEndpoints
{
    private const string ContentType = "application/json";
    private const string Tag = "Books";
    private const string BaseRoute = "books";

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(BaseRoute, CreateBookAsync)
            .WithName("CreateBook")
            .Accepts<Book>(ContentType)
            .Produces<Book>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapGet(BaseRoute, GetBooksAsync)
            .WithName("GetBooks")
            .Produces<IEnumerable<Book>>(200)
            .WithTags(Tag);

        app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByISBNAsync)
            .WithName("GetBook")
            .Produces<Book>(200)
            .Produces(404)
            .WithTags(Tag);

        app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
            .WithName("UpdateBook")
            .Accepts<Book>(ContentType)
            .Produces<Book>(200)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
            .WithName("DeleteBook")
            .Produces(204)
            .Produces(404)
            .WithTags(Tag);
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBookService, BookService>();
    }

    internal static async Task<IResult> GetBooksAsync(string? searchTerm, IBookService service)
    {
        var books = await service.GetAsync(searchTerm);
        return Results.Ok(books);
    }

    internal static async Task<IResult> GetBookByISBNAsync(string isbn, IBookService service)
    {
        var book = await service.GetByISBNAsync(isbn);
        return book is not null ? Results.Ok(book) : Results.NotFound(isbn);
    }

    internal static async Task<IResult> CreateBookAsync(Book book, IBookService service, IValidator<Book> validator)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var created = await service.CreateAsync(book);
        if (created)
            return Results.Created($"/{BaseRoute}/{book.Isbn}", book);

        return Results.BadRequest(new List<ValidationFailure>
            {
                new ("Isbn", "A book with this ISBN-13 already exists")
            });
    }

    internal static async Task<IResult> UpdateBookAsync(string isbn, Book book, IBookService service, IValidator<Book> validator)
    {
        book.Isbn = isbn;

        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var updated = await service.UpdateAsync(book);
        if (!updated)
        {
            return Results.NotFound();
        }

        return Results.Ok(book);
    }

    internal static async Task<IResult> DeleteBookAsync(string isbn, IBookService service)
    {
        var deleted = await service.DeleteAsync(isbn);
        return deleted ? Results.NoContent() : Results.NotFound(isbn);
    }
}