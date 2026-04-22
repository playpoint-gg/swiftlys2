using System.Text;
using System.Buffers;
using System.Runtime.InteropServices;
using Spectre.Console;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;

namespace SwiftlyS2.Core.Natives;

internal static class GameFunctions
{
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static unsafe delegate* unmanaged< CTakeDamageInfo*, nint, nint, nint, Vector*, Vector*, float, int, int, void*, void > pCTakeDamageInfo_Constructor;
    public static unsafe delegate* unmanaged< nint, CTakeDamageInfo*, CTakeDamageResult*, void > pTakeDamage;
    public static unsafe delegate* unmanaged< nint, Ray_t*, Vector*, Vector*, CTraceFilter*, CGameTrace*, void > pTraceShape;
    public static unsafe delegate* unmanaged< Vector*, Vector*, BBox_t*, CTraceFilter*, CGameTrace*, void > pTracePlayerBBox;
    public static unsafe delegate* unmanaged< nint, IntPtr, nint > pSetModel;
    public static unsafe delegate* unmanaged< nint, nint, byte, byte, byte, byte, void > pSetPlayerControllerPawn;
    public static unsafe delegate* unmanaged< nint, nint, float, void > pSetOrAddAttribute;
    public static unsafe delegate* unmanaged< int, nint, nint > pGetWeaponCSDataFromKey;
    public static unsafe delegate* unmanaged< nint, uint, nint, byte, CUtlSymbolLarge, byte, int, nint, nint, void > pDispatchParticleEffect;
    public static unsafe delegate* unmanaged< nint, float, uint, nint, void > pTerminateRoundWindows;
    public static unsafe delegate* unmanaged< nint, uint, nint, float, void > pTerminateRoundLinux;
    public static unsafe delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, int, nint > pCSmokeGrenadeProjectileEmitGrenade;
    public static unsafe delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, nint > pCFlashbangProjectileEmitGrenade;
    public static unsafe delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, nint > pCHEGrenadeProjectileEmitGrenade;
    public static unsafe delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, nint > pCDecoyProjectileEmitGrenade;
    public static unsafe delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, nint > pCMolotovProjectileEmitGrenade;
    public static unsafe delegate* unmanaged< nint, int, void > pSwitchTeam;
    private static Lazy<int> CreateOffset( string name ) => new(() => NativeOffsets.Fetch(name));
    private static readonly Lazy<int> _teleportOffset = CreateOffset("CBaseEntity::Teleport");
    private static readonly Lazy<int> _commitSuicideOffset = CreateOffset("CBasePlayerPawn::CommitSuicide");
    private static readonly Lazy<int> _getSkeletonInstanceOffset = CreateOffset("CGameSceneNode::GetSkeletonInstance");
    private static readonly Lazy<int> _findPickerEntityOffset = CreateOffset("CGameRules::FindPickerEntity");
    private static readonly Lazy<int> _removeWeaponsOffset = CreateOffset("CCSPlayer_ItemServices::RemoveWeapons");
    private static readonly Lazy<int> _giveNamedItemOffset = CreateOffset("CCSPlayer_ItemServices::GiveNamedItem");
    private static readonly Lazy<int> _dropActiveItemOffset = CreateOffset("CCSPlayer_ItemServices::DropActiveItem");
    private static readonly Lazy<int> _dropWeaponOffset = CreateOffset("CCSPlayer_WeaponServices::DropWeapon");
    private static readonly Lazy<int> _selectWeaponOffset = CreateOffset("CCSPlayer_WeaponServices::SelectWeapon");
    private static readonly Lazy<int> _addResourceOffset = CreateOffset("CEntityResourceManifest::AddResource");
    private static readonly Lazy<int> _collisionRulesChangedOffset = CreateOffset("CBaseEntity::CollisionRulesChanged");
    private static readonly Lazy<int> _respawnOffset = CreateOffset("CCSPlayerController::Respawn");
    private static readonly Lazy<int> _getViewVectorsOffset = CreateOffset("CGameRules::GetViewVectors");
    private static readonly Lazy<int> _goToIntermissionOffset = CreateOffset("CGameRules::GoToIntermission");
    private static readonly Lazy<int> _changeTeamOffset = CreateOffset("CCSPlayerController::ChangeTeam");

    public static int TeleportOffset => _teleportOffset.Value;
    public static int CommitSuicideOffset => _commitSuicideOffset.Value;
    public static int GetSkeletonInstanceOffset => _getSkeletonInstanceOffset.Value;
    public static int FindPickerEntityOffset => _findPickerEntityOffset.Value;
    public static int RemoveWeaponsOffset => _removeWeaponsOffset.Value;
    public static int GiveNamedItemOffset => _giveNamedItemOffset.Value;
    public static int DropActiveItemOffset => _dropActiveItemOffset.Value;
    public static int DropWeaponOffset => _dropWeaponOffset.Value;
    public static int SelectWeaponOffset => _selectWeaponOffset.Value;
    public static int AddResourceOffset => _addResourceOffset.Value;
    public static int CollisionRulesChangedOffset => _collisionRulesChangedOffset.Value;
    public static int RespawnOffset => _respawnOffset.Value;
    public static int GetViewVectorsOffset => _getViewVectorsOffset.Value;
    public static int GoToIntermissionOffset => _goToIntermissionOffset.Value;
    public static int ChangeTeamOffset => _changeTeamOffset.Value;

    private static void CheckPtr( nint ptr, string name )
    {
        if (ptr == 0)
        {
            throw new ArgumentException($"Invalid pointer: {name}={ptr}");
        }
    }

    private static unsafe void CheckPtr( void* ptr, string name )
    {
        CheckPtr((nint)ptr, name);
    }

    private static unsafe void* FetchSig(string name)
    {
        try
        {
            var addr = (void*)NativeSignatures.Fetch(name);
            if (addr == null)
                AnsiConsole.MarkupLine($"[red][[SwiftlyS2.GameFunctions]][/] sig '[yellow]{name}[/]' -> [red]NULL[/] (missing in gamedata or pattern not found)");
            else
                AnsiConsole.MarkupLine($"[grey][[SwiftlyS2.GameFunctions]][/] sig '{name}' -> 0x{(nint)addr:X}");
            return addr;
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red][[SwiftlyS2.GameFunctions]][/] sig fetch '{name}' threw: {e.Message.Replace("[", "[[").Replace("]", "]]")}");
            return null;
        }
    }

    public static void Initialize()
    {
        unsafe
        {
            pCTakeDamageInfo_Constructor = (delegate* unmanaged< CTakeDamageInfo*, nint, nint, nint, Vector*, Vector*, float, int, int, void*, void >)FetchSig("CTakeDamageInfo::Constructor");
            pTakeDamage = (delegate* unmanaged< nint, CTakeDamageInfo*, CTakeDamageResult*, void >)FetchSig("CBaseEntity::TakeDamage");
            pTraceShape = (delegate* unmanaged< nint, Ray_t*, Vector*, Vector*, CTraceFilter*, CGameTrace*, void >)FetchSig("TraceShape");
            pTracePlayerBBox = (delegate* unmanaged< Vector*, Vector*, BBox_t*, CTraceFilter*, CGameTrace*, void >)FetchSig("TracePlayerBBox");
            pSetModel = (delegate* unmanaged< nint, IntPtr, nint >)FetchSig("CBaseModelEntity::SetModel");
            pSetPlayerControllerPawn = (delegate* unmanaged< nint, nint, byte, byte, byte, byte, void >)FetchSig("CBasePlayerController::SetPawn");
            pSetOrAddAttribute = (delegate* unmanaged< nint, IntPtr, float, void >)FetchSig("CAttributeList::SetOrAddAttributeValueByName");
            pGetWeaponCSDataFromKey = (delegate* unmanaged< int, nint, nint >)FetchSig("GetWeaponCSDataFromKey");
            pDispatchParticleEffect = (delegate* unmanaged< nint, uint, nint, byte, CUtlSymbolLarge, byte, int, nint, nint, void >)FetchSig("DispatchParticleEffect");
            pCSmokeGrenadeProjectileEmitGrenade = (delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, int, nint >)FetchSig("CSmokeGrenadeProjectile::EmitGrenade");
            pCFlashbangProjectileEmitGrenade = (delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, nint >)FetchSig("CFlashbangProjectile::EmitGrenade");
            pCHEGrenadeProjectileEmitGrenade = (delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, nint >)FetchSig("CHEGrenadeProjectile::EmitGrenade");
            pCDecoyProjectileEmitGrenade = (delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, nint >)FetchSig("CDecoyProjectile::EmitGrenade");
            pCMolotovProjectileEmitGrenade = (delegate* unmanaged< Vector*, QAngle*, Vector*, Vector*, nint, uint, nint >)FetchSig("CMolotovProjectile::EmitGrenade");
            pSwitchTeam = (delegate* unmanaged< nint, int, void >)FetchSig("CCSPlayerController::SwitchTeam");
            if (IsWindows)
                pTerminateRoundWindows = (delegate* unmanaged< nint, float, uint, nint, void >)FetchSig("CGameRules::TerminateRound");
            else
                pTerminateRoundLinux = (delegate* unmanaged< nint, uint, nint, float, void >)FetchSig("CGameRules::TerminateRound");
        }
    }

    public static unsafe void* GetVirtualFunction( nint handle, int offset )
    {
        var ppVTable = (void***)handle;
        return *(*ppVTable + offset);
    }

    public static void DispatchParticleEffect( string particleName, uint attachmentType, nint entity, byte attachmentPoint, CUtlSymbolLarge attachmentName, bool resetAllParticlesOnEntity, int splitScreenSlot, CRecipientFilter filter )
    {
        try
        {
            NativeEngineHelpers.DispatchParticleEffect(particleName, attachmentType, entity, attachmentPoint, attachmentName._pString, resetAllParticlesOnEntity, splitScreenSlot, filter.ToMask());
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void TerminateRound( nint gameRules, uint reason, float delay, uint teamId )
    {
        try
        {
            CheckPtr(gameRules, nameof(gameRules));
            unsafe
            {
                if (IsWindows)
                {
                    if (pTerminateRoundWindows == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pTerminateRoundWindows is NULL -- skipping call (sig/offset failed at init)"); return; }
                    pTerminateRoundWindows(gameRules, delay, reason, teamId > 0 ? (nint)(&teamId) : 0);
                }
                else
                {
                    if (pTerminateRoundLinux == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pTerminateRoundLinux is NULL -- skipping call (sig/offset failed at init)"); return; }
                    pTerminateRoundLinux(gameRules, reason, teamId > 0 ? (nint)(&teamId) : 0, delay);
                }
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void GoToIntermission( nint gameRules, bool bAbortedMatch )
    {
        try
        {
            CheckPtr(gameRules, nameof(gameRules));
            unsafe
            {
                var pGoToIntermission = (delegate* unmanaged< nint, byte, void >)GetVirtualFunction(gameRules, GoToIntermissionOffset);
                pGoToIntermission(gameRules, (byte)(bAbortedMatch ? 1 : 0));
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static nint GetWeaponCSDataFromKey( int unknown, string key )
    {
        try
        {
            unsafe
            {
                return StringAlloc.CreateCString(key, pKey =>
                {
                    if (pGetWeaponCSDataFromKey == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pGetWeaponCSDataFromKey is NULL -- skipping call (sig/offset failed at init)"); return 0; }
                    return pGetWeaponCSDataFromKey(unknown, pKey);
                });
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 0;
        }
    }

    public static nint FindPickerEntity( nint handle, nint controller )
    {
        try
        {
            unsafe
            {
                CheckPtr(handle, nameof(handle));
                CheckPtr(controller, nameof(controller));
                var vfunc = (delegate* unmanaged< nint, nint, nint, nint >)GetVirtualFunction(handle, FindPickerEntityOffset);
                return vfunc(handle, controller, IntPtr.Zero);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
        return 0;
    }

    public static nint GetSkeletonInstance( nint handle )
    {
        try
        {
            CheckPtr(handle, nameof(handle));
            unsafe
            {
                var pSkeletonInstance = (delegate* unmanaged< nint, nint >)GetVirtualFunction(handle, GetSkeletonInstanceOffset);
                return pSkeletonInstance(handle);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
        return 0;
    }

    public static void PawnCommitSuicide( nint pPawn, bool bExplode, bool bForce )
    {
        try
        {
            CheckPtr(pPawn, nameof(pPawn));
            unsafe
            {
                var pCommitSuicide = (delegate* unmanaged< nint, byte, byte, void >)GetVirtualFunction(pPawn, CommitSuicideOffset);
                pCommitSuicide(pPawn, (byte)(bExplode ? 1 : 0), (byte)(bForce ? 1 : 0));
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void SetPlayerControllerPawn( nint pController, nint pPawn, bool b1, bool b2, bool b3, bool b4 )
    {
        try
        {
            CheckPtr(pController, nameof(pController));
            unsafe
            {
                if (pSetPlayerControllerPawn == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pSetPlayerControllerPawn is NULL -- skipping call (sig/offset failed at init)"); return; }
                pSetPlayerControllerPawn(pController, pPawn, (byte)(b1 ? 1 : 0), (byte)(b2 ? 1 : 0), (byte)(b3 ? 1 : 0), (byte)(b4 ? 1 : 0));
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void SetModel( nint pEntity, string model )
    {
        try
        {
            CheckPtr(pEntity, nameof(pEntity));

            StringAlloc.CreateCString(model, pModel =>
            {
                unsafe
                {
                    if (pSetModel == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pSetModel is NULL -- skipping SetModel call (sig/offset failed at init)"); return; }
                    _ = pSetModel(pEntity, pModel);
                }
            });
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static unsafe void Teleport(
        nint pEntity,
        Vector* vecPosition,
        QAngle* vecAngle,
        Vector* vecVelocity
    )
    {
        try
        {
            CheckPtr(pEntity, nameof(pEntity));
            unsafe
            {
                var pTeleport = (delegate* unmanaged< nint, Vector*, QAngle*, Vector*, void >)GetVirtualFunction(pEntity, TeleportOffset);
                if (pTeleport == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pTeleport is NULL -- skipping call (sig/offset failed at init)"); return; }
                pTeleport(pEntity, vecPosition, vecAngle, vecVelocity);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    private static unsafe bool Is16Aligned( CGameTrace* pTrace ) => ((nuint)pTrace & 15) == 0;

    public static unsafe void TracePlayerBBox(
        Vector vecStart,
        Vector vecEnd,
        BBox_t bounds,
        CTraceFilter* pFilter,
        CGameTrace* pTrace
    )
    {
        try
        {
            CheckPtr(pTrace, nameof(pTrace));
            unsafe
            {
                if (pTracePlayerBBox == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pTracePlayerBBox is NULL -- skipping call (sig/offset failed at init)"); return; }
                // FUCK ALL OF YOU SHIT
                if (IsWindows || Is16Aligned(pTrace))
                {
                    pTracePlayerBBox(&vecStart, &vecEnd, &bounds, pFilter, pTrace);
                }
                else
                {
                    var size = (nuint)sizeof(CGameTrace);
                    var rawBuffer = stackalloc byte[(int)size + 16];
                    var pAligned = (CGameTrace*)(((nuint)rawBuffer + 15) & ~(nuint)15);
                    NativeMemory.Copy(pTrace, pAligned, size);
                    pTracePlayerBBox(&vecStart, &vecEnd, &bounds, pFilter, pAligned);
                    NativeMemory.Copy(pAligned, pTrace, size);
                }
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static unsafe void TraceShape(
        nint pEngineTrace,
        Ray_t* ray,
        Vector vecStart,
        Vector vecEnd,
        CTraceFilter* pFilter,
        CGameTrace* pTrace
    )
    {
        try
        {
            unsafe
            {
                CheckPtr(pEngineTrace, nameof(pEngineTrace));
                CheckPtr(pTrace, nameof(pTrace));
                if (pTraceShape == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pTraceShape is NULL -- skipping call (sig/offset failed at init)"); return; }
                // FUCK YOU WINDOWS
                if (IsWindows || Is16Aligned(pTrace))
                {
                    pTraceShape(pEngineTrace, ray, &vecStart, &vecEnd, pFilter, pTrace);
                }
                // FUCK YOU LINUX SIMD ALIGNMENT
                else
                {
                    var size = (nuint)sizeof(CGameTrace);
                    var rawBuffer = stackalloc byte[(int)size + 16];
                    var pAligned = (CGameTrace*)(((nuint)rawBuffer + 15) & ~(nuint)15);
                    NativeMemory.Copy(pTrace, pAligned, size);
                    pTraceShape(pEngineTrace, ray, &vecStart, &vecEnd, pFilter, pAligned);
                    NativeMemory.Copy(pAligned, pTrace, size);
                }
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static unsafe void CTakeDamageInfoConstructor(
        CTakeDamageInfo* pThis,
        nint pInflictor,
        nint pAttacker,
        nint pAbility,
        Vector* vecDamageForce,
        Vector* vecDamagePosition,
        float flDamage,
        int bitsDamageType,
        int iCustomDamage,
        void* a10
    )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                if (pCTakeDamageInfo_Constructor == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pCTakeDamageInfo_Constructor is NULL -- skipping call (sig/offset failed at init)"); return; }
                pCTakeDamageInfo_Constructor(pThis, pInflictor, pAttacker, pAbility, vecDamageForce, vecDamagePosition, flDamage, bitsDamageType, iCustomDamage, a10);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void CCSPlayerControllerChangeTeam( nint controller, Team team )
    {
        try
        {
            unsafe
            {
                CheckPtr(controller, nameof(controller));
                var pChangeTeam = (delegate* unmanaged< nint, int, void >)GetVirtualFunction(controller, ChangeTeamOffset);
                pChangeTeam(controller, (int)team);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void CCSPlayerControllerSwitchTeam( nint controller, Team team )
    {
        try
        {
            unsafe
            {
                CheckPtr(controller, nameof(controller));
                pSwitchTeam(controller, (int)team);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static unsafe void TakeDamage( nint pEntity, CTakeDamageInfo* info )
    {
        try
        {
            CheckPtr(pEntity, nameof(pEntity));
            unsafe
            {
                if (pTakeDamage == null) { AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pTakeDamage is NULL -- skipping call (sig/offset failed at init)"); return; }
                pTakeDamage(pEntity, info, (CTakeDamageResult*)0);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void CCSPlayer_ItemServices_RemoveWeapons( nint pThis )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                var pRemoveWeapons = (delegate* unmanaged< nint, void >)GetVirtualFunction(pThis, RemoveWeaponsOffset);
                pRemoveWeapons(pThis);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static nint CCSPlayer_ItemServices_GiveNamedItem( nint pThis, string name )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                var ppVTable = (void***)pThis;
                var pGiveNamedItem = (delegate* unmanaged< nint, nint, nint >)ppVTable[0][GiveNamedItemOffset];
                return StringAlloc.CreateCString(name, pName =>
                {
                    return pGiveNamedItem(pThis, pName);
                });
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 0;
        }
    }

    public static void CCSPlayer_ItemServices_DropActiveItem( nint pThis, Vector momentum )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                var pDropActiveItem = (delegate* unmanaged< nint, Vector*, void >)GetVirtualFunction(pThis, DropActiveItemOffset);
                pDropActiveItem(pThis, &momentum);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static unsafe void CCSPlayer_WeaponServices_DropWeapon( nint pThis, nint pWeapon, Vector* momentum )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                CheckPtr(pWeapon, nameof(pWeapon));
                var pDropWeapon = (delegate* unmanaged< nint, nint, nint, Vector*, void >)GetVirtualFunction(pThis, DropWeaponOffset);
                pDropWeapon(pThis, pWeapon, 0, momentum);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void CCSPlayer_WeaponServices_SelectWeapon( nint pThis, nint pWeapon )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                CheckPtr(pWeapon, nameof(pWeapon));
                var pSelectWeapon = (delegate* unmanaged< nint, nint, void >)GetVirtualFunction(pThis, SelectWeaponOffset);
                pSelectWeapon(pThis, pWeapon);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void CEntityResourceManifest_AddResource( nint pThis, string path )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                var pAddResource = (delegate* unmanaged< nint, nint, void >)GetVirtualFunction(pThis, AddResourceOffset);
                StringAlloc.CreateCString(path, pPath =>
                {
                    pAddResource(pThis, pPath);
                });
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void SetOrAddAttribute( nint handle, string name, float value )
    {
        try
        {
            unsafe
            {
                CheckPtr(handle, nameof(handle));
                StringAlloc.CreateCString(name, pName =>
                {
                    if (pSetOrAddAttribute == null)
                    {
                        AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pSetOrAddAttribute is NULL -- skipping call (sig/offset failed at init)");
                        return;
                    }
                    pSetOrAddAttribute(handle, (nint)pName, value);
                });
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void CBaseEntity_CollisionRulesChanged( nint pThis )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                var pCollisionRulesChanged = (delegate* unmanaged< nint, void >)GetVirtualFunction(pThis, CollisionRulesChangedOffset);
                pCollisionRulesChanged(pThis);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static void CCSPlayerController_Respawn( nint pThis )
    {
        try
        {
            CheckPtr(pThis, nameof(pThis));
            unsafe
            {
                var pRespawn = (delegate* unmanaged< nint, void >)GetVirtualFunction(pThis, RespawnOffset);
                pRespawn(pThis);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    public static nint CSmokeGrenadeProjectile_EmitGrenade( Vector pos, QAngle angle, Vector velocity, nint owner, Team team, uint itemdefindex )
    {
        try
        {
            unsafe
            {
                if (pCSmokeGrenadeProjectileEmitGrenade == null)
                {
                    AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pCSmokeGrenadeProjectileEmitGrenade is NULL -- skipping call (sig/offset failed at init)");
                    return 0;
                }
                return pCSmokeGrenadeProjectileEmitGrenade(&pos, &angle, &velocity, &velocity, owner, itemdefindex, (int)team);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 0;
        }
    }

    public static nint CFlashbangProjectile_EmitGrenade( Vector pos, QAngle angle, Vector velocity, nint owner, uint itemdefindex )
    {
        try
        {
            unsafe
            {
                if (pCFlashbangProjectileEmitGrenade == null)
                {
                    AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pCFlashbangProjectileEmitGrenade is NULL -- skipping call (sig/offset failed at init)");
                    return 0;
                }
                return pCFlashbangProjectileEmitGrenade(&pos, &angle, &velocity, &velocity, owner, itemdefindex);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 0;
        }
    }

    public static nint CHEGrenadeProjectile_EmitGrenade( Vector pos, QAngle angle, Vector velocity, nint owner, uint itemdefindex )
    {
        try
        {
            unsafe
            {
                if (pCHEGrenadeProjectileEmitGrenade == null)
                {
                    AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pCHEGrenadeProjectileEmitGrenade is NULL -- skipping call (sig/offset failed at init)");
                    return 0;
                }
                return pCHEGrenadeProjectileEmitGrenade(&pos, &angle, &velocity, &velocity, owner, itemdefindex);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 0;
        }
    }

    public static nint CDecoyProjectile_EmitGrenade( Vector pos, QAngle angle, Vector velocity, nint owner, uint itemdefindex )
    {
        try
        {
            unsafe
            {
                if (pCDecoyProjectileEmitGrenade == null)
                {
                    AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pCDecoyProjectileEmitGrenade is NULL -- skipping call (sig/offset failed at init)");
                    return 0;
                }
                return pCDecoyProjectileEmitGrenade(&pos, &angle, &velocity, &velocity, owner, itemdefindex);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 0;
        }
    }

    public static nint CMolotovProjectile_EmitGrenade( Vector pos, QAngle angle, Vector velocity, nint owner, uint itemdefindex )
    {
        try
        {
            unsafe
            {
                if (pCMolotovProjectileEmitGrenade == null)
                {
                    AnsiConsole.MarkupLine("[red][[SwiftlyS2.GameFunctions]][/] pCMolotovProjectileEmitGrenade is NULL -- skipping call (sig/offset failed at init)");
                    return 0;
                }
                return pCMolotovProjectileEmitGrenade(&pos, &angle, &velocity, &velocity, owner, itemdefindex);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 0;
        }
    }

    public static unsafe CViewVectors* CGameRules_GetViewVectors( nint pThis )
    {
        try
        {
            unsafe
            {
                CheckPtr(pThis, nameof(pThis));
                var pGetViewVectors = (delegate* unmanaged< nint, CViewVectors* >)GetVirtualFunction(pThis, GetViewVectorsOffset);
                return pGetViewVectors(pThis);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return null;
        }
    }
}
