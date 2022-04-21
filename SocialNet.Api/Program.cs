using FluentValidation;
using FluentValidation.Results;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SocialNet.Api.Models;
using SocialNetSample.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<SocialNetDbContext>(
    options => options.UseSqlServer(builder.Configuration["ConnectionStrings:Default"]))
    .AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/users", async (SocialNetDbContext db) =>
{
    var users = await db.Users.ProjectToType<UserOutDto>().ToListAsync();
    return Results.Ok(users);
})
.WithTags("Users")
.WithName("GetAllUsers");

app.MapPost("/users", async (UserInDto userInDto, SocialNetDbContext db, IValidator<UserInDto> validator) =>
{
    // Validation
    var validationResult = await validator.ValidateAsync(userInDto);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    // Create new user with input params
    User user = new User()
    {
        Email = userInDto.Email,
    };

    // Check if provided id already exists
    //if (await db.Users.AsNoTracking().Where(u => u.Id == user.Id).FirstOrDefaultAsync() is not null)
    //    return Results.Conflict($"Conflict: user with id {user.Id} already exists.");

    // Check if provided email already exists
    if (await db.Users.AsNoTracking().Where(u => u.Email == user.Email).FirstOrDefaultAsync() is not null)
        return Results.Conflict($"Conflict: user with email {user.Email} already exists.");


    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created(
        $"users/{user.Id}",
        user.Adapt<UserOutDto>());
})
.WithTags("Users")
.WithName("CreateUser")
.Produces<IEnumerable<ValidationFailure>>(400)
.Produces<string>(StatusCodes.Status409Conflict)
.Produces<UserOutDto>(StatusCodes.Status201Created);


/* Friend Requests */
app.MapPost("/friendrequests", async (FriendRequestInDto friendRequestInDto, SocialNetDbContext db) =>
{
    // Validation
    // TODO

    var sender = await db.Users.Where(u => u.Id == friendRequestInDto.SourceId).SingleOrDefaultAsync();
    var receiver = await db.Users.Where(u => u.Id == friendRequestInDto.TargetId).SingleOrDefaultAsync();

    // Check the sender user exists
    if (sender is null)
        return Results.NotFound($"User with id {friendRequestInDto.SourceId} does not exist and thus cannot send a friend request.");

    // Check received user exists
    if (receiver is null)
        return Results.NotFound($"User with id {friendRequestInDto.TargetId} does not exist and thus cannot receive a friend request.");

    // Both users exist, check sender does not block receiver
    if (await db.BlockedUsers.AsNoTracking().Where(bu => bu.SourceId == friendRequestInDto.SourceId && bu.TargetId == friendRequestInDto.TargetId).FirstOrDefaultAsync() is not null)
        return Results.Conflict($"User with id {friendRequestInDto.SourceId} has blocked user with id {friendRequestInDto.TargetId} and no friend request is awolled. Unblock action is first needed.");

    // Both users exist, check receiver does not block sender
    if (await db.BlockedUsers.AsNoTracking().Where(bu => bu.SourceId == friendRequestInDto.TargetId && bu.TargetId == friendRequestInDto.SourceId).FirstOrDefaultAsync() is not null)
        return Results.Conflict($"User with id {friendRequestInDto.SourceId} has been blocked by user with id {friendRequestInDto.TargetId} and no friend request is awolled. Unblock action is first needed.");

    // Check they are not already friends
    if (await db.Friends.AsNoTracking().Where(f => (f.SourceId == friendRequestInDto.SourceId && f.TargetId == friendRequestInDto.TargetId) || (f.SourceId == friendRequestInDto.TargetId && f.TargetId == friendRequestInDto.SourceId)).FirstOrDefaultAsync() is not null)
        return Results.Conflict($"Both users are already friends and no friend request is awolled.");

    // Check sender has not sent a FR already
    if (await db.FriendRequests.AsNoTracking().Where(fr => fr.SourceId == friendRequestInDto.SourceId && fr.TargetId == friendRequestInDto.TargetId).FirstOrDefaultAsync() is not null)
        return Results.Conflict($"User with id {friendRequestInDto.SourceId} has already sent a friend request to user with id {friendRequestInDto.TargetId} and no friend request is awolled.");

    // Check sender has not received a FR already
    if (await db.FriendRequests.AsNoTracking().Where(fr => fr.SourceId == friendRequestInDto.TargetId && fr.TargetId == friendRequestInDto.SourceId).FirstOrDefaultAsync() is not null)
        return Results.Conflict($"User with id {friendRequestInDto.SourceId} has already received a friend request from user with id {friendRequestInDto.TargetId} and no friend request is awolled.");


    FriendRequest friendRequest = new FriendRequest()
    {
        SourceId = sender.Id,
        TargetId = receiver.Id,
    };

    db.FriendRequests.Add(friendRequest);
    await db.SaveChangesAsync();

    return Results.Created(
        $"friendrequests/{friendRequest.Id}",
        friendRequest.Adapt<FriendRequestOutDto>());
})
.WithTags("FriendRequests")
.WithName("CreateFriendRequest")
.Produces<IEnumerable<ValidationFailure>>(400)
.Produces<string>(StatusCodes.Status404NotFound)
.Produces<string>(StatusCodes.Status409Conflict)
.Produces<FriendRequestOutDto>(StatusCodes.Status201Created);

app.MapGet("/friendrequests", async (SocialNetDbContext db) =>
{
    var friendRequests = await db.FriendRequests.ProjectToType<FriendRequestOutDto>().AsNoTracking().ToListAsync();
    return Results.Ok(friendRequests);
})
.WithTags("FriendRequests")
.WithName("GetAllFriendRequests");

app.MapGet("/friendrequests/{id}", async (Guid id, SocialNetDbContext db) =>
{
    var friendRequest = await db.FriendRequests.AsNoTracking().Where(u => u.Id == id).SingleOrDefaultAsync();

    if (friendRequest is null)
        return Results.NotFound($"Friend request with id {id} does not exists.");

    return Results.Ok(friendRequest.Adapt<FriendRequestOutDto>());
})
.WithTags("FriendRequests")
.WithName("GetFriendRequestById")
.Produces<FriendRequestOutDto>(StatusCodes.Status200OK);

app.MapGet("/friendrequests/user/{id}", async (Guid id, SocialNetDbContext db) =>
{
    // Find all rows where source/target matches the provided GUID
    // Source means the GUID user sent the FR
    // Target means the GUID user received the FR
    var friendRequests = await db.FriendRequests.ProjectToType<FriendRequestOutDto>().AsNoTracking().Where(u => u.SourceId == id || u.TargetId == id).ToListAsync();

    return Results.Ok(friendRequests);
})
.WithTags("FriendRequests")
.WithName("GetFriendRequestByUserId")
.Produces<List<FriendRequestOutDto>>(StatusCodes.Status200OK);

app.MapGet("/friendrequests/source/{id}", async (Guid id, SocialNetDbContext db) =>
{
    // Find all rows where source matches the provided GUID
    // Source means the GUID user sent the FR
    var friendRequests = await db.FriendRequests.ProjectToType<FriendRequestOutDto>().AsNoTracking().Where(u => u.SourceId == id).ToListAsync();

    return Results.Ok(friendRequests);
})
.WithTags("FriendRequests")
.WithName("GetFriendRequestBySourceId")
.Produces<List<FriendRequestOutDto>>(StatusCodes.Status200OK);

app.MapGet("/friendrequests/target/{id}", async (Guid id, SocialNetDbContext db) =>
{
    // Find all rows where target matches the provided GUID
    // Target means the GUID user received the FR
    var friendRequests = await db.FriendRequests.ProjectToType<FriendRequestOutDto>().AsNoTracking().Where(u => u.TargetId == id).ToListAsync();

    return Results.Ok(friendRequests);
})
.WithTags("FriendRequests")
.WithName("GetFriendRequestByTargetId")
.Produces<List<FriendRequestOutDto>>(StatusCodes.Status200OK);

/* FRIENDS */
app.MapGet("/friends", async (SocialNetDbContext db) =>
{
    var friends = await db.Friends.ProjectToType<FriendOutDto>().AsNoTracking().ToListAsync();

    return Results.Ok(friends);
})
.WithTags("Friends")
.WithName("GetAllFriends")
.Produces<List<FriendOutDto>>(StatusCodes.Status200OK);

// When a UE sends a FR, it also sends a event to KAFKA
// User service should be subscribed to that event
// Upon receiving, the user service shall create a
app.MapPost("/friends", async (FriendInDto friendInDto, SocialNetDbContext db) =>
{
    // Validation
    // TODO

    var sender = await db.Users.Where(u => u.Id == friendInDto.SourceId).SingleOrDefaultAsync();
    var receiver = await db.Users.Where(u => u.Id == friendInDto.TargetId).SingleOrDefaultAsync();

    // Check the sender user exists
    if (sender is null)
        return Results.NotFound($"User with id {friendInDto.SourceId} does not exist and thus cannot be the source of a friendship.");

    // Check receiver user exists
    if (receiver is null)
        return Results.NotFound($"User with id {friendInDto.TargetId} does not exist and thus cannot be the target of a friendship.");


    var friend = new Friend()
    {
        SourceId = friendInDto.SourceId,
        TargetId = friendInDto.TargetId,
    };

    // NOTE 1: you can also do this if you have previously loaded those entities (sender, receiver):
    //var friend = new Friend()
    //{
    //    Source = sender,
    //    Target = receiver,
    //};

    // NOTE 2: the Id (which is a Guid) is automatically generated when you insert it to the DB below
    // NOTE 3: the CreatedAt (which is a DateTimeOffset) is automatically generated when you insert it to the DB below
    // NOTE 4: all missing fields (Id, Source, Target, CreatedAt) OR (Id, SourceId, TargetId, CreatedAt)
    // will be present in "friend" object below after .Add() command
    // if sender, receiver objects where loaded first.

    db.Friends.Add(friend);
    await db.SaveChangesAsync();

    return Results.Ok(friend.Adapt<FriendOutDto>());
})
.WithTags("Friends")
.WithName("CreateFriend")
.Produces<IEnumerable<ValidationFailure>>(400)
.Produces<string>(StatusCodes.Status404NotFound)
.Produces<string>(StatusCodes.Status409Conflict)
.Produces<FriendOutDto>(StatusCodes.Status201Created);

app.MapGet("/friends/user/{id}", async (Guid id, SocialNetDbContext db) =>
{
    // Generated SQL seems ok (inner selects are needed since we only work with non-deleted users):

    //SELECT
    //  [f].[Id],
    //  [f].[CreatedAt],
    //  [f].[SourceId],
    //  [t].[Email] AS[SourceEmail],
    //  [f].[TargetId],
    //  [t0].[Email] AS[TargetEmail]
    //FROM[Friends] AS[f]
    //INNER JOIN(
    //    SELECT[u].[Id], [u].[Email]
    //    FROM [Users] AS [u]
    //    WHERE [u].[Deleted] = CAST(0 AS bit)
    //) AS[t] ON[f].[SourceId] = [t].[Id]
    //INNER JOIN(
    //    SELECT[u0].[Id], [u0].[Email]
    //    FROM [Users] AS [u0]
    //    WHERE [u0].[Deleted] = CAST(0 AS bit)
    //) AS[t0] ON[f].[TargetId] = [t0].[Id]
    //WHERE([f].[SourceId] = @__id_0) OR([f].[TargetId] = @__id_0)

    var friends = await db.Friends
        .Where(u => u.SourceId == id || u.TargetId == id)
        .Include(s => s.Source)
        .Include(t => t.Target)
        .Select(x => new FriendOutDto
        {
            Id = x.Id,
            CreatedAt = x.CreatedAt,
            SourceId = x.SourceId,
            SourceEmail = x.Source.Email,
            TargetId = x.TargetId,
            TargetEmail = x.Target.Email
        })
        .ToListAsync();

    return Results.Ok(friends);
})
.WithTags("Friends")
.WithName("GetFriendsByUserId")
.Produces<List<FriendOutDto>>(StatusCodes.Status200OK);

app.Run();
