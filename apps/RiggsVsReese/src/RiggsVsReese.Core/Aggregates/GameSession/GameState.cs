namespace RiggsVsReese.Core.Aggregates.GameSession;

using RiggsVsReese.Core.Items.Weapons;

public sealed class GameState
{
    public PlayerState Player { get; } = new();

    public GameState()
    {
        var weaponDefinition = WeaponCatalog.All.Single(
            weapon => weapon.Id == "beretta-92f");
        var magazineDefinition = new MagazineDefinition
        {
            Id = "beretta-92f-magazine",
            Name = "Magasin till Beretta 92F",
            Capacity = 15
        };

        var loadedMagazine = new Magazine(magazineDefinition);
        var spareMagazine = new Magazine(magazineDefinition);

        Player.EquippedWeapon = new Weapon(weaponDefinition, loadedMagazine);
        Player.Inventory.Add(spareMagazine);
    }
}

public sealed class PlayerState
{
    /* Koppla den konkreta Character-instansen till PlayerState/GameState här. */
    public Weapon? EquippedWeapon { get; internal set; }
    public List<Magazine> Inventory { get; } = new();
}
