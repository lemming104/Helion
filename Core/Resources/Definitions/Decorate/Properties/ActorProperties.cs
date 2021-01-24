using System;
using System.Collections.Generic;
using System.Drawing;
using Helion.Maps.Specials;
using Helion.Resources.Definitions.Decorate.Properties.Enums;

namespace Helion.Resources.Definitions.Decorate.Properties
{
    public class ActorProperties
    {
        public int? Accuracy;
        public DecorateSpecialActivationType? Activation;
        public string? ActiveSound;
        public double? Alpha;
        public AmmoProperty Ammo = default;
        public SpecialArgs? Args;
        public ArmorProperty Armor = default;
        public string? AttackSound;
        public Color? BloodColor;
        public string? BloodType;
        public int? BounceCount;
        public double? BounceFactor;
        public string? BounceSound;
        public DecorateBounceType? BounceType;
        public double? BurnHeight;
        public double? CameraHeight;
        public int? ConversationID;
        public string? CrushPainSound;
        public DamageRangeProperty Damage = default;
        public DamageFactor? DamageFactor;
        public string? DamageType;
        public double? DeathHeight;
        public string? DeathSound;
        public string? DeathType;
        public string? Decal;
        public int? DefThreshold = 100;
        public int? DesignatedTeam;
        public string? DistanceCheck;
        public DropItemProperty DropItem = default;
        public int? ExplosionDamage;
        public int? ExplosionRadius;
        public FakeInventoryProperty FakeInventoryProperty = default;
        public int? FastSpeed;
        public double? FloatBobPhase;
        public double? FloatBobStrength;
        public double? FloatSpeed;
        public double? Friction;
        public int? FriendlySeeBlocks;
        public string? Game;
        public int? GibHealth;
        public double? Gravity;
        public int? Health;
        public HealthProperty HealthProperty = default;
        public HealthPickupAutoUse? HealthPickupAutoUse;
        public double? Height;
        public string? HitObituary;
        public string? HowlSound;
        public InventoryProperty Inventory = default;
        public double? Mass;
        public double? MaxDropOffHeight;
        public double? MaxStepHeight;
        public int? MaxTargetRange;
        public int? MeleeDamage;
        public int? MeleeRange;
        public string? MeleeSound;
        public int? MeleeThreshold;
        public int? MinMissileChance;
        public int? MissileHeight;
        public int? MissileType;
        public MorphProjectileProperty MorphProjectile = default;
        public string? Obituary;
        public PainChanceProperty? PainChance;
        public string? PainSound;
        public int? PainThreshold;
        public string? PainType;
        public PlayerProperty Player = default;
        public PoisonDamageProperty PoisonDamage = default;
        public string? PoisonDamageType;
        public PowerupProperty Powerup = default;
        public int? ProjectileKickBack;
        public int? ProjectilePassHeight;
        public double? PushFactor;
        public PuzzleItemProperty PuzzleItem = default;
        public double? Radius;
        public double? RadiusDamageFactor;
        public int? ReactionTime;
        public double? RenderRadius;
        public RenderStyle? RenderStyle;
        public int? RipLevelMax;
        public int? RipLevelMin;
        public int? RipperLevel;
        public double? Scale;
        public string? SeeSound;
        public double? SelfDamageFactor;
        public int? SpawnId;
        public string? Species;
        public int? Speed;
        public int? SpriteAngle;
        public int? SpriteRotation;
        public int? Stamina;
        public double? StealthAlpha;
        public int? StencilColor;
        public string? Tag;
        public string? TeleFogDestType;
        public string? TeleFogSourceType;
        public int? Threshold;
        public List<string>? Translation;
        public int? VSpeed;
        public Range? VisibleAngles;
        public Range? VisiblePitch;
        public int? VisibleToTeam;
        public List<string>? VisibleToPlayerClass;
        public double? WallBounceFactor;
        public string? WallBounceSound;
        public WeaponPiecesProperty WeaponPieces = default;
        public WeaponProperty Weapons = default;
        public Dictionary<int, HashSet<string>>? WeaponSlot;
        public int? WeaveIndexXY;
        public int? WeaveIndexZ;
        public int? WoundHealth;
        public double? XScale;
        public double? YScale;
    }
}