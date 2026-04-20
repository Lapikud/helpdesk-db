using App.DAL.EF;
using App.DAL.EF.Repositories;
using App.DAL.EF.Repositories.Identity;
using App.Domain;

namespace App.Tests.UnitTests.App;

[Collection("NonParallel")]
public class AppUOWTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public AppUOWTest(TestDatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void AllRepositoryProperties_ReturnExpectedConcreteType()
    {
        var uow = new AppUOW(_fixture.CreateContext());

        Assert.IsType<AssetRepository>(uow.AssetRepository);
        Assert.IsType<CategoryRepository>(uow.CategoryRepository);
        Assert.IsType<CategoryAssetsRepository>(uow.CategoryAssetsRepository);
        Assert.IsType<CupboardRepository>(uow.CupboardRepository);
        Assert.IsType<CupboardInRoomRepository>(uow.CupboardInRoomRepository);
        Assert.IsType<LocationRepository>(uow.LocationRepository);
        Assert.IsType<LocationInCupboardRepository>(uow.LocationInCupboardRepository);
        Assert.IsType<LocationAssetsRepository>(uow.LocationAssetsRepository);
        Assert.IsType<OwnerRepository>(uow.OwnerRepository);
        Assert.IsType<OwnerAssetsRepository>(uow.OwnerAssetsRepository);
        Assert.IsType<RemovedAssetsRepository>(uow.RemovedAssetsRepository);
        Assert.IsType<RoomRepository>(uow.RoomRepository);
        Assert.IsType<AppUserRoleRepository>(uow.AppUserRoleRepository);
        Assert.IsType<AssetReservationRepository>(uow.AssetReservationRepository);
    }

    [Fact]
    public void EachRepositoryProperty_IsCached()
    {
        var uow = new AppUOW(_fixture.CreateContext());

        Assert.Same(uow.AssetRepository, uow.AssetRepository);
        Assert.Same(uow.CategoryRepository, uow.CategoryRepository);
        Assert.Same(uow.CategoryAssetsRepository, uow.CategoryAssetsRepository);
        Assert.Same(uow.CupboardRepository, uow.CupboardRepository);
        Assert.Same(uow.CupboardInRoomRepository, uow.CupboardInRoomRepository);
        Assert.Same(uow.LocationRepository, uow.LocationRepository);
        Assert.Same(uow.LocationInCupboardRepository, uow.LocationInCupboardRepository);
        Assert.Same(uow.LocationAssetsRepository, uow.LocationAssetsRepository);
        Assert.Same(uow.OwnerRepository, uow.OwnerRepository);
        Assert.Same(uow.OwnerAssetsRepository, uow.OwnerAssetsRepository);
        Assert.Same(uow.RemovedAssetsRepository, uow.RemovedAssetsRepository);
        Assert.Same(uow.RoomRepository, uow.RoomRepository);
        Assert.Same(uow.AppUserRoleRepository, uow.AppUserRoleRepository);
        Assert.Same(uow.AssetReservationRepository, uow.AssetReservationRepository);
    }

    [Fact]
    public async Task SaveChangesAsync_DelegatesToContext_AndPersists()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);

        var asset = new DAL.DTO.Asset
        {
            Id = Guid.NewGuid(), AssetName = "UOWTest", Comment = "comment", SerialNumber = "sn", Barcode = "bc"
        };
        await uow.AssetRepository.AddAsync(asset);
        var saved = await uow.SaveChangesAsync();

        Assert.Equal(1, saved);

        context.ChangeTracker.Clear();
        var persisted = await context.Assets.FindAsync(asset.Id);
        Assert.NotNull(persisted);
        Assert.Equal("UOWTest", persisted!.AssetName);
    }
}
