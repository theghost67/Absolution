using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Menus;
using Game.Territories;
using Game.Traits;
using GreenOne;
using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Статический класс, предоставляющий методы и расширения для различных игровых ситуаций.
    /// </summary>
    public static class GameUtils
    {
        #region descriptions
        public static string DescRich(this ITableCard card)
        {
            return card.Data.DescRich(card);
        }
        public static string DescRich(this ITableTrait trait)
        {
            return trait.Data.DescRich(trait);
        }
        #endregion

        #region guids
        // you can use level of inheritance as index to separate events base subs from derived subs
        public static string GuidStrForEvents(this ITableObject obj, int index = 0)
        {
            return $"{obj.GuidStr}:{index}";
        }
        public static string GuidStrForEvents(this Drawer drawer, int index = 0)
        {
            if (drawer.attached is TableObject obj)
                return $"{obj.GuidStr}.drawer:{index}";
            else return $"def.drawer:{index}";
        }
        public static string GuidStrForEvents(this BattleArea area, int index = 0)
        {
            return $"{area.observer.GuidStr}.area:{index}";
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

        public static TablePassiveTrait Passive(this TableTraitListSet traits, string id)
        {
            return traits.Passives[id]?.Trait ?? null;
        }
        public static TableActiveTrait Active(this TableTraitListSet traits, string id)
        {
            return traits.Actives[id]?.Trait ?? null;
        }

        public static BattlePassiveTrait Passive(this BattleTraitListSet traits, string id)
        {
            return traits.Passives[id]?.Trait ?? null;
        }
        public static BattleActiveTrait Active(this BattleTraitListSet traits, string id)
        {
            return traits.Actives[id]?.Trait ?? null;
        }

        // NOTE: card death does NOT affect stacks
        public static bool WasAdded(this ITableTrait trait, TableTraitStacksSetArgs e)
        {
            return WasAdded(trait, e.delta);
        }
        public static bool WasRemoved(this ITableTrait trait, TableTraitStacksSetArgs e)
        {
            return WasRemoved(trait, e.delta);
        }

        public static bool WasAdded(this ITableTrait trait, in int stacksDelta)
        {
            return trait.GetStacks() - stacksDelta == 0;
        }
        public static bool WasRemoved(this ITableTrait trait, in int stacksDelta)
        {
            int stacks = trait.GetStacks();
            if (stacks <= 0)
                 return stacks - stacksDelta > 0;
            else return false;
        }

        public static bool WasAdded(this ITableTraitListElement element, in int stacksDelta)
        {
            return element.Stacks - stacksDelta == 0;
        }
        public static bool WasRemoved(this ITableTraitListElement element, in int stacksDelta)
        {
            int stacks = element.Stacks;
            if (stacks <= 0)
                 return stacks - stacksDelta > 0;
            else return false;
        }

        public static bool WasAdded(this TraitListElement element, in int stacksDelta)
        {
            return element.Stacks - stacksDelta == 0;
        }
        public static bool WasRemoved(this TraitListElement element, in int stacksDelta)
        {
            int stacks = element.Stacks;
            if (stacks <= 0)
                return stacks - stacksDelta > 0;
            else return false;
        }

        // TODO: implement all anims
        public static UniTask AnimActivation(this ITableTrait trait)
        {
            if (trait.Owner == null)
                return UniTask.CompletedTask;

            TableConsole.LogToFile("card", $"{trait.TableNameDebug}: activation.");

            ITableTraitListElement element;
            if (trait.Data.isPassive)
                 element = trait.Owner.Traits.Passives[trait.Data.id];
            else element = trait.Owner.Traits.Actives[trait.Data.id];

            if (element == null || element.Drawer == null)
                return UniTask.CompletedTask;

            Menu.WriteLogToCurrent($"{trait.TableName}: навык активируется!");
            return element.Drawer.AnimActivation().AsyncWaitForCompletion();
        }
        public static UniTask AnimDeactivation(this ITableTrait trait)
        {
            if (trait.Owner == null)
                return UniTask.CompletedTask;

            TableConsole.LogToFile("card", $"{trait.TableNameDebug}: deactivation.");
            //if (trait.Drawer != null)
            //    Menu.WriteLogToCurrent($"{trait.TableName}: навык деактивируется.");

            return UniTask.CompletedTask;
        }

        // show WHO detected the card (arrow + icon), and eye (clear/crossed) depending on seen/unseen status
        public static UniTask AnimCardSeen(this IBattleObject observer, IBattleCard card)
        {
            TableConsole.LogToFile("card", $"{observer.TableNameDebug}: area: {card.TableNameDebug} found.");
            return UniTask.CompletedTask;
        }
        public static UniTask AnimCardUnseen(this IBattleObject observer, IBattleCard card)
        {
            TableConsole.LogToFile("card", $"{observer.TableNameDebug}: area: {card.TableNameDebug} lost.");
            return UniTask.CompletedTask;
        }
        #endregion

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

        public static string StatToStringRich(this TableStat stat, int defaultValue)
        {
            Color statColor;
            if (stat > defaultValue)
                statColor = Color.green;
            else if (stat < defaultValue)
                statColor = Color.red;
            else statColor = Color.white;

            return $"{stat.ToString().Colored(statColor)} ({stat.PosValue.Rounded(2)} * {(stat.PosScale * 100).Rounded(2)}% - {stat.NegValue.Rounded(2)})";
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
