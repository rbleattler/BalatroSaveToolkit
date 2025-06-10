using System.Collections.ObjectModel;
using System.Windows;

namespace BalatroSaveExplorer.Controls
{
  public partial class SearchableTreeViewDemo : Window
  {
    public ObservableCollection<TreeNodeViewModel> SampleData { get; set; } = new();

    public SearchableTreeViewDemo()
    {
      InitializeComponent();
      DataContext = this;
      CreateSampleData();
    }

    private void CreateSampleData()
    {
      SampleData = new ObservableCollection<TreeNodeViewModel>
            {
                new TreeNodeViewModel
                {
                    DisplayName = "Jokers",
                    ValueDisplay = "Collection",
                    Children =
                    {
                        new TreeNodeViewModel { DisplayName = "Joker", ValueDisplay = "Base joker card" },
                        new TreeNodeViewModel { DisplayName = "Greedy Joker", ValueDisplay = "+$3 per played Diamond" },
                        new TreeNodeViewModel { DisplayName = "Lusty Joker", ValueDisplay = "+$3 per played Club" },
                        new TreeNodeViewModel { DisplayName = "Wrathful Joker", ValueDisplay = "+$3 per played Heart" },
                        new TreeNodeViewModel { DisplayName = "Gluttonous Joker", ValueDisplay = "+$3 per played Spade" }
                    }
                },
                new TreeNodeViewModel
                {
                    DisplayName = "Consumables",
                    ValueDisplay = "Items",
                    Children =
                    {
                        new TreeNodeViewModel
                        {
                            DisplayName = "Tarots",
                            ValueDisplay = "Cards",
                            Children =
                            {
                                new TreeNodeViewModel { DisplayName = "The Fool", ValueDisplay = "Create 2 Planet cards" },
                                new TreeNodeViewModel { DisplayName = "The Magician", ValueDisplay = "Enhance 2 cards" },
                                new TreeNodeViewModel { DisplayName = "The High Priestess", ValueDisplay = "Create 2 Planet cards" }
                            }
                        },
                        new TreeNodeViewModel
                        {
                            DisplayName = "Planets",
                            ValueDisplay = "Cards",
                            Children =
                            {
                                new TreeNodeViewModel { DisplayName = "Mercury", ValueDisplay = "Level up Pair" },
                                new TreeNodeViewModel { DisplayName = "Venus", ValueDisplay = "Level up Two Pair" },
                                new TreeNodeViewModel { DisplayName = "Earth", ValueDisplay = "Level up Full House" }
                            }
                        }
                    }
                },
                new TreeNodeViewModel
                {
                    DisplayName = "Blinds",
                    ValueDisplay = "Challenges",
                    Children =
                    {
                        new TreeNodeViewModel { DisplayName = "Small Blind", ValueDisplay = "$300 reward" },
                        new TreeNodeViewModel { DisplayName = "Big Blind", ValueDisplay = "$500 reward" },
                        new TreeNodeViewModel { DisplayName = "Boss Blind", ValueDisplay = "Special challenge" }
                    }
                },
                new TreeNodeViewModel
                {
                    DisplayName = "Deck",
                    ValueDisplay = "Playing Cards",
                    Children =
                    {
                        new TreeNodeViewModel { DisplayName = "Ace of Spades", ValueDisplay = "11 or 1 points" },
                        new TreeNodeViewModel { DisplayName = "King of Hearts", ValueDisplay = "10 points" },
                        new TreeNodeViewModel { DisplayName = "Queen of Diamonds", ValueDisplay = "10 points" },
                        new TreeNodeViewModel { DisplayName = "Jack of Clubs", ValueDisplay = "10 points" }
                    }
                }
            };
    }
  }
}
