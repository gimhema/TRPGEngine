using System;
using System.Collections.Generic;
using System.Text;

namespace RuleForge
{
    /// <summary>
    /// 상점 클래스. TrpgGameState 기반으로 구매/판매 UI를 구동한다.
    /// </summary>
    public class TrpgShop
    {
        public string ShopName { get; set; }

        /// <summary>
        /// 상점 판매 아이템 목록.
        /// TODO: RulebookParser에서 상점별 판매 아이템을 등록
        /// </summary>
        public List<TrpgItem> Merchandise { get; set; }

        /// <summary>
        /// 판매 시 가격 비율 (기본: 0.5 → 구매가의 50%)
        /// </summary>
        public double SellRatio { get; set; } = 0.5;

        public TrpgShop(string name)
        {
            ShopName = name;
            Merchandise = new List<TrpgItem>();
        }

        /// <summary>
        /// 아이템의 판매가를 계산한다.
        /// </summary>
        public int GetSellPrice(TrpgItem item)
        {
            return Math.Max(1, (int)(item.Price * SellRatio));
        }

        // ─── 상점 메인 메뉴 ───

        /// <summary>
        /// 상점에 진입한다. Shop 씬으로 전환하고 메인 메뉴를 표시한다.
        /// </summary>
        public void Enter(TrpgGameState state)
        {
            if (state.CurrentScene != TrpgGameState.SceneType.Shop)
            {
                state.ChangeScene(TrpgGameState.SceneType.Shop);
            }

            ShowMainMenu(state);
        }

        /// <summary>
        /// 상점 메인 메뉴를 표시한다.
        /// </summary>
        public void ShowMainMenu(TrpgGameState state)
        {
            var player = state.CurrentPlayer;
            int gold = player?.Gold ?? 0;

            state.NarrativeText = $"===== {ShopName} =====\n소지금: {gold} G";
            state.ClearChoices();

            state.AddChoice(new TrpgChoice("1", "1. 구매")
            {
                OnSelect = (s) => ShowBuyMenu(s)
            });
            state.AddChoice(new TrpgChoice("2", "2. 판매")
            {
                OnSelect = (s) => ShowSellMenu(s)
            });
            state.AddChoice(new TrpgChoice("3", "3. 상점 나가기")
            {
                OnSelect = (s) => s.ReturnToPreviousScene()
            });
        }

        // ─── 구매 ───

        /// <summary>
        /// 구매 메뉴를 표시한다. 상점 판매 아이템 목록과 가격을 보여준다.
        /// </summary>
        private void ShowBuyMenu(TrpgGameState state)
        {
            var player = state.CurrentPlayer;
            if (player == null) return;

            int gold = player.Gold;

            if (Merchandise.Count == 0)
            {
                state.NarrativeText = $"===== {ShopName} - 구매 =====\n소지금: {gold} G\n\n판매 중인 아이템이 없습니다.";
                state.ClearChoices();
                state.AddChoice(new TrpgChoice("1", "1. 돌아가기")
                {
                    OnSelect = (s) => ShowMainMenu(s)
                });
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"===== {ShopName} - 구매 =====");
            sb.AppendLine($"소지금: {gold} G");
            sb.AppendLine();

            state.NarrativeText = sb.ToString();
            state.ClearChoices();

            int index = 1;
            for (int i = 0; i < Merchandise.Count; i++)
            {
                int itemIndex = i;
                var item = Merchandise[i];
                bool canAfford = gold >= item.Price;
                string label = canAfford
                    ? $"{index}. {item.ItemName} - {item.Price} G  ({item.ItemDescription})"
                    : $"{index}. {item.ItemName} - {item.Price} G  (골드 부족)";

                state.AddChoice(new TrpgChoice(index.ToString(), label)
                {
                    OnSelect = (s) => BuyItem(s, itemIndex)
                });
                index++;
            }

            state.AddChoice(new TrpgChoice(index.ToString(), $"{index}. 돌아가기")
            {
                OnSelect = (s) => ShowMainMenu(s)
            });
        }

        /// <summary>
        /// 아이템을 구매한다.
        /// </summary>
        private void BuyItem(TrpgGameState state, int itemIndex)
        {
            var player = state.CurrentPlayer;
            if (player == null) return;

            if (itemIndex < 0 || itemIndex >= Merchandise.Count)
            {
                ShowBuyMenu(state);
                return;
            }

            var item = Merchandise[itemIndex];

            if (player.Gold < item.Price)
            {
                state.NarrativeText = $"골드가 부족합니다! (필요: {item.Price} G, 소지: {player.Gold} G)";
                state.ClearChoices();
                state.AddChoice(new TrpgChoice("1", "1. 돌아가기")
                {
                    OnSelect = (s) => ShowBuyMenu(s)
                });
                return;
            }

            // 골드 차감
            player.Gold -= item.Price;

            // 아이템 타입에 따라 인벤토리에 추가
            if (item is Consumable consumable)
            {
                // 소비 아이템은 새 인스턴스로 복제하여 지급 (효과 포함)
                var newItem = new Consumable(consumable.ItemName, consumable.ItemDescription, consumable.Price)
                {
                    HealHP = consumable.HealHP,
                    RestoreMP = consumable.RestoreMP,
                    Quantity = consumable.Quantity > 0 ? consumable.Quantity : 1
                };
                player.playerItemBag.AcquireConsumable(newItem);
            }
            else if (item is Equipment equipment)
            {
                var newItem = new Equipment(equipment.ItemName, equipment.ItemDescription, equipment.Price);
                player.playerItemBag.AcquireEquipment(newItem);
            }
            else if (item is KeyItem keyItem)
            {
                var newItem = new KeyItem(keyItem.ItemName, keyItem.ItemDescription, keyItem.Price);
                player.playerItemBag.KeyItems.Add(newItem);
            }

            state.NarrativeText = $"{item.ItemName}을(를) 구매했습니다! (소지금: {player.Gold} G)";
            state.ClearChoices();
            state.AddChoice(new TrpgChoice("1", "1. 계속 구매")
            {
                OnSelect = (s) => ShowBuyMenu(s)
            });
            state.AddChoice(new TrpgChoice("2", "2. 돌아가기")
            {
                OnSelect = (s) => ShowMainMenu(s)
            });
        }

        // ─── 판매 ───

        /// <summary>
        /// 판매 메뉴를 표시한다. 플레이어 보유 아이템을 카테고리별로 보여준다.
        /// </summary>
        private void ShowSellMenu(TrpgGameState state)
        {
            var player = state.CurrentPlayer;
            if (player == null) return;

            var bag = player.playerItemBag;
            bool hasItems = bag.ConsumableItems.Count > 0 || bag.EquipmentItems.Count > 0;

            if (!hasItems)
            {
                state.NarrativeText = $"===== {ShopName} - 판매 =====\n소지금: {player.Gold} G\n\n판매할 아이템이 없습니다.";
                state.ClearChoices();
                state.AddChoice(new TrpgChoice("1", "1. 돌아가기")
                {
                    OnSelect = (s) => ShowMainMenu(s)
                });
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"===== {ShopName} - 판매 =====");
            sb.AppendLine($"소지금: {player.Gold} G");
            sb.AppendLine();

            state.NarrativeText = sb.ToString();
            state.ClearChoices();

            int index = 1;

            // 소비 아이템
            for (int i = 0; i < bag.ConsumableItems.Count; i++)
            {
                int itemIndex = i;
                var item = bag.ConsumableItems[i];
                int sellPrice = GetSellPrice(item);
                state.AddChoice(new TrpgChoice(index.ToString(),
                    $"{index}. [소비] {item.ItemName} x{item.Quantity} → {sellPrice} G")
                {
                    OnSelect = (s) => SellConsumable(s, itemIndex)
                });
                index++;
            }

            // 장비 아이템
            for (int i = 0; i < bag.EquipmentItems.Count; i++)
            {
                int itemIndex = i;
                var item = bag.EquipmentItems[i];
                int sellPrice = GetSellPrice(item);
                state.AddChoice(new TrpgChoice(index.ToString(),
                    $"{index}. [장비] {item.ItemName} → {sellPrice} G")
                {
                    OnSelect = (s) => SellEquipment(s, itemIndex)
                });
                index++;
            }

            state.AddChoice(new TrpgChoice(index.ToString(), $"{index}. 돌아가기")
            {
                OnSelect = (s) => ShowMainMenu(s)
            });
        }

        /// <summary>
        /// 소비 아이템을 판매한다.
        /// </summary>
        private void SellConsumable(TrpgGameState state, int itemIndex)
        {
            var player = state.CurrentPlayer;
            if (player == null) return;

            var bag = player.playerItemBag;
            if (itemIndex < 0 || itemIndex >= bag.ConsumableItems.Count)
            {
                ShowSellMenu(state);
                return;
            }

            var item = bag.ConsumableItems[itemIndex];
            int sellPrice = GetSellPrice(item);

            player.Gold += sellPrice;
            bag.DropConsumable(itemIndex);

            state.NarrativeText = $"{item.ItemName}을(를) {sellPrice} G에 판매했습니다! (소지금: {player.Gold} G)";
            state.ClearChoices();
            state.AddChoice(new TrpgChoice("1", "1. 계속 판매")
            {
                OnSelect = (s) => ShowSellMenu(s)
            });
            state.AddChoice(new TrpgChoice("2", "2. 돌아가기")
            {
                OnSelect = (s) => ShowMainMenu(s)
            });
        }

        /// <summary>
        /// 장비 아이템을 판매한다.
        /// </summary>
        private void SellEquipment(TrpgGameState state, int itemIndex)
        {
            var player = state.CurrentPlayer;
            if (player == null) return;

            var bag = player.playerItemBag;
            if (itemIndex < 0 || itemIndex >= bag.EquipmentItems.Count)
            {
                ShowSellMenu(state);
                return;
            }

            var item = bag.EquipmentItems[itemIndex];
            int sellPrice = GetSellPrice(item);

            player.Gold += sellPrice;
            bag.DropEquipment(itemIndex);

            state.NarrativeText = $"{item.ItemName}을(를) {sellPrice} G에 판매했습니다! (소지금: {player.Gold} G)";
            state.ClearChoices();
            state.AddChoice(new TrpgChoice("1", "1. 계속 판매")
            {
                OnSelect = (s) => ShowSellMenu(s)
            });
            state.AddChoice(new TrpgChoice("2", "2. 돌아가기")
            {
                OnSelect = (s) => ShowMainMenu(s)
            });
        }
    }
}
