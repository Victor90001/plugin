using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using System.Text.Json.Serialization;

namespace HelloWorldPlugin;
public class DeathmatchConfig : BasePluginConfig
{
    [JsonPropertyName("Config")] public PlayersSettings playersSettings { get; set; } = new PlayersSettings();
}
public class PlayersSettings
{
    [JsonPropertyName("reffil_ammo_kill")] public bool RefillAmmo { get; set; } = true;
    [JsonPropertyName("reffil_ammo_headshot")] public bool RefillAmmoHS { get; set; } = true;
    [JsonPropertyName("refill_health_kill")] public int KillHealth { get; set; } = 20;
    [JsonPropertyName("refill_health_headshot")] public int HeadshotHealth { get; set; } = 40;
}
public class DMRefill: BasePlugin, IPluginConfig<DeathmatchConfig>
{
    public override string ModuleName => "Deathmatch Refill Plugin";
    public override string ModuleAuthor => "vurc";
    public override string ModuleVersion => "0.0.2";

    public DeathmatchConfig Config { get; set; }

    public override void Load(bool hotReload)
    {
        Console.WriteLine("DM Refill Plugin Started!");
    }

    public void OnConfigParsed(DeathmatchConfig config)
    {
        Config = config;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var player = @event.Userid;
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        bool IsHeadshot = @event.Headshot;
        bool IsKnifeKill = @event.Weapon.Contains("knife");
        if (attacker != player && attacker.PlayerPawn.Value != null)
        {
            var Health = IsHeadshot ? Config.playersSettings.HeadshotHealth : Config.playersSettings.KillHealth;
            var refillAmmo = IsHeadshot ? Config.playersSettings.RefillAmmoHS : Config.playersSettings.RefillAmmo;

            var giveHP = 100 >= attacker.PlayerPawn.Value.Health + Health ? Health : 100 - attacker.PlayerPawn.Value.Health;
            if (refillAmmo)
            {
                var activeWeapon = attacker.PlayerPawn.Value.WeaponServices?.ActiveWeapon.Value;
                if (activeWeapon != null)
                {
                    activeWeapon.Clip1 = 250;
                    activeWeapon.ReserveAmmo[0] = 250;
                }
            }
            if (giveHP > 0)
            {
                attacker.PlayerPawn.Value.Health += giveHP;
                Utilities.SetStateChanged(attacker.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
            }
            @event.FireEventToClient(attacker);
        }
        return HookResult.Continue;
    }
}
