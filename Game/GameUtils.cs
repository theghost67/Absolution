using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Menus;
using Game.Territories;
using Game.Traits;
using GreenOne;
using MyBox;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Статический класс, предоставляющий методы и расширения для различных игровых ситуаций.
    /// </summary>
    public static class GameUtils
    {
        #region guids
        // these methods generate guids for object's event handlers and collections
        // you can use 'num' as you like (separate events base subs from derived subs or if you subscribe multiple handlers from one object to one event)

        public static string GuidGen(this ITableObject obj, int num)
        {
            return $"{obj.GuidStr}:{num}";
        }
        public static string GuidGen(this Drawer drawer, int num)
        {
            if (drawer.attached is TableObject obj)
                return $"{obj.GuidStr}.drawer:{num}";
            else return $"def.drawer:{num}";
        }
        public static string GuidGen(this BattleArea area, int num)
        {
            return $"{area.observer.GuidStr}.area:{num}";
        }
        #endregion

        #region traits
        public static PassiveTrait Passive(this TraitListSet traits, string id)
        {
            return traits.Passives[id]?.Trait ?? null;
        }
        public static ActiveTrait Active(this TraitListSet traits, string id)
        {
            return traits.Actives[id]?.Trait ?? null;
        }
        public static Trait Any(this TraitListSet traits, string id)
        {
            return traits[id]?.Trait ?? null;
        }

        public static TablePassiveTrait Passive(this TableTraitListSet traits, string id)
        {
            return traits.Passives[id]?.Trait ?? null;
        }
        public static TableActiveTrait Active(this TableTraitListSet traits, string id)
        {
            return traits.Actives[id]?.Trait ?? null;
        }
        public static ITableTrait Any(this TableTraitListSet traits, string id)
        {
            return traits[id]?.Trait ?? null;
        }

        public static BattlePassiveTrait Passive(this BattleTraitListSet traits, string id)
        {
            return traits.Passives[id]?.Trait ?? null;
        }
        public static BattleActiveTrait Active(this BattleTraitListSet traits, string id)
        {
            return traits.Actives[id]?.Trait ?? null;
        }
        public static IBattleTrait Any(this BattleTraitListSet traits, string id)
        {
            return traits[id]?.Trait ?? null;
        }

        public static bool WasAdded(this ITableTrait trait, TableTraitStacksSetArgs e)
        {
            return WasTraitAdded(e.traitStacks, e.delta);
        }
        public static bool WasAdded(this ITableTrait trait, in int stacksDelta)
        {
            return WasTraitAdded(trait.GetStacks(), stacksDelta);
        }
        public static bool WasAdded(this ITableTraitListElement element, in int stacksDelta)
        {
            return WasTraitAdded(element.Stacks, stacksDelta);
        }
        public static bool WasAdded(this TraitListElement element, in int stacksDelta)
        {
            return WasTraitAdded(element.Stacks, stacksDelta);
        }

        public static bool WasRemoved(this ITableTrait trait, TableTraitStacksSetArgs e)
        {
            return WasTraitRemoved(e.traitStacks, e.delta);
        }
        public static bool WasRemoved(this ITableTrait trait, in int stacksDelta)
        {
            return WasTraitRemoved(trait.GetStacks(), stacksDelta);
        }
        public static bool WasRemoved(this ITableTraitListElement element, in int stacksDelta)
        {
            return WasTraitRemoved(element.Stacks, stacksDelta);
        }
        public static bool WasRemoved(this TraitListElement element, in int stacksDelta)
        {
            return WasTraitRemoved(element.Stacks, stacksDelta);
        }

        public static bool WasTraitAdded(int stacks, int delta)
        {
            return stacks - delta == 0;
        }
        public static bool WasTraitRemoved(int stacks, int delta)
        {
            return stacks <= 0 && stacks - delta > 0;
        }

        public static UniTask AnimActivation(this TableActiveTraitUseArgs e)
        {
            return AnimActivation(e.trait, e.target);
        }
        public static UniTask AnimDeactivation(this TableActiveTraitUseArgs e)
        {
            return AnimDeactivation(e.trait, e.target);
        }

        public static async UniTask AnimActivation(this ITableTrait trait, TableField target = null)
        {
            if (trait == null || trait.Owner == null)
                return;

            TableConsole.LogToFile("card", $"{trait.TableNameDebug}: activation.");
            if (trait.Owner.Drawer == null)
                return;

            Menu.WriteLogToCurrent($"{trait.TableName}: навык активируется!");
            trait.Owner.Drawer.queue.Enqueue(new TableFieldCardDrawerQueueActivation(trait, target, true));
            await trait.Owner.Drawer.queue.Await();
        }
        public static async UniTask AnimDeactivation(this ITableTrait trait, TableField target = null)
        {
            if (trait == null || trait.Owner == null)
                return;

            TableConsole.LogToFile("card", $"{trait.TableNameDebug}: deactivation.");
            if (trait.Owner.Drawer == null)
                return;

            Menu.WriteLogToCurrent($"{trait.TableName}: навык деактивируется.");
            trait.Owner.Drawer.queue.Enqueue(new TableFieldCardDrawerQueueActivation(trait, target, false));
            await trait.Owner.Drawer.queue.Await();
        }

        public static async UniTask AnimDetectionOnSeen(this ITableTrait trait, BattleFieldCard seenCard)
        {
            if (trait == null || trait.Owner == null)
                return;

            TableConsole.LogToFile("card", $"{trait.TableNameDebug}: card seen.");
            if (trait.Owner.Drawer == null)
                return;

            Menu.WriteLogToCurrent($"{trait.TableName}: карта обнаружена.");
            trait.Owner.Drawer.queue.Enqueue(new TableFieldCardDrawerQueueDetection(trait, seenCard.LastField, true));
            await trait.Owner.Drawer.queue.Await();
        }
        public static async UniTask AnimDetectionOnUnseen(this ITableTrait trait, BattleFieldCard unseenCard)
        {
            if (trait == null || trait.Owner == null)
                return;

            TableConsole.LogToFile("card", $"{trait.TableNameDebug}: card unseen.");
            if (trait.Owner.Drawer == null)
                return;

            Menu.WriteLogToCurrent($"{trait.TableName}: карта потеряна.");
            trait.Owner.Drawer.queue.Enqueue(new TableFieldCardDrawerQueueDetection(trait, unseenCard.LastField, false));
            await trait.Owner.Drawer.queue.Await();
        }

        public static void SetCooldown(this ITableTrait trait, int cooldown)
        {
            if (trait == null) return;
            trait.TurnDelay = cooldown;
        }
        public static bool IsOnCooldown(this ITableTrait trait)
        {
            if (trait != null)
                 return trait.TurnDelay > 0;
            else throw new System.NullReferenceException();
        }
        #endregion

        public static bool HasInitiationPreview(this TableFieldCard card)
        {
            return card.Field?.Drawer.HasInitiationPreview() ?? false;
        }
        public static bool HasInitiationPreview(this TableField field)
        {
            return field.Drawer.HasInitiationPreview();
        }
        public static bool HasInitiationPreview(this TableFieldCardDrawer cardDrawer)
        {
            return cardDrawer.attached.Field?.Drawer.HasInitiationPreview() ?? false;
        }
        public static bool HasInitiationPreview(this TableFieldDrawer fieldDrawer)
        {
            if (fieldDrawer == null) return false;
            bool result = fieldDrawer.transform.Find("Initiation") != null;
            if (result) return result;
            result = fieldDrawer.attached.Card?.Drawer?.transform.Find("Initiation") != null;
            return result;
        }

        public static IEnumerable<T> WithCard<T>(this IEnumerable<T> collection) where T : TableField
        {
            foreach (T field in collection)
            {
                if (field.Card != null)
                    yield return field;
            }
        }
        public static IEnumerable<T> WithoutCard<T>(this IEnumerable<T> collection) where T : TableField
        {
            foreach (T field in collection)
            {
                if (field.Card == null)
                    yield return field;
            }
        }

        public static bool IsMine(this TableField field)
        {
            if ((field.Territory?.Grid.y ?? 1) == 1)
                return true;
            return field.pos.y == BattleTerritory.PLAYER_FIELDS_Y;
        }
        public static bool IsMine(this IBattleObject obj)
        {
            return obj.Side.isMe;
        }

        public static Color GetSideColor(this BattleSide side)
        {
            return side.isMe ? Color.green : Color.red;
        }
        public static BattleFieldCard AsBattleFieldCard(this ITableEntrySource source)
        {
            BattleFieldCard killer = source switch
            {
                BattleFieldCard c => c,
                IBattleTrait t => t.Owner,
                _ => null,
            };

            if (killer == null || killer.IsKilled)
                return null;
            else return killer;
        }
    }
}
