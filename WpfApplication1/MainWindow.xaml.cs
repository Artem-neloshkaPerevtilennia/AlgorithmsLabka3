using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void AddRecordOnClick(object sender, RoutedEventArgs e) =>
			CreateInput("Add", AddRecord, 3);

		private void AddRecord(object sender, RoutedEventArgs e)
		{
			if (Input.Children.Count > 0)
			{
				TextBox keyBox = (TextBox)FindName("text0");
				TextBox valueBox = (TextBox)FindName("text1");

				if (keyBox != null &&
				    valueBox != null &&
				    !string.IsNullOrWhiteSpace(keyBox.Text) &&
				    !string.IsNullOrWhiteSpace(valueBox.Text))
				{
					if (int.TryParse(keyBox.Text, out int key))
					{
						string value = valueBox.Text;

						if (!Files.AddToIndexArea("block", key, value))
						{
							MessageBox.Show("This key exists already! Please enter a valid integer.", "Error", MessageBoxButton.OK,
								MessageBoxImage.Error);
						}
					}
					else
					{
						MessageBox.Show("Invalid key! Please enter a valid integer.", "Error", MessageBoxButton.OK,
							MessageBoxImage.Error);
					}
				}
				else
				{
					MessageBox.Show("One or both text fields are missing or empty.", "Error", MessageBoxButton.OK,
						MessageBoxImage.Warning);
				}
			}
		}

		private void FindRecordOnClick(object sender, RoutedEventArgs e) =>
			CreateInput("Find", FindRecord, 2);

		private void FindRecord(object sender, RoutedEventArgs e)
		{
			if (Input.Children.Count > 0)
			{
				for (int i = 0; i < Input.Children.Count; i++)
				{
					if (Input.Children[i] is TextBlock)
					{
						Input.Children.Remove(Input.Children[i]);
					}
				}

				TextBox keyBox = (TextBox)FindName("text0");

				if (keyBox != null && !string.IsNullOrWhiteSpace(keyBox.Text))
				{
					if (int.TryParse(keyBox.Text, out int key))
					{
						string record = FindKeyInIndexArea("block", key) ?? "This record doesn't exist";
						TextBlock foundRecord = new TextBlock
						{
							Text = record,
							Margin = new Thickness(5)
						};

						Input.Children.Add(foundRecord);
					}
					else
					{
						MessageBox.Show("Invalid key! Please enter a valid integer.", "Error", MessageBoxButton.OK,
							MessageBoxImage.Error);
					}
				}
				else
				{
					MessageBox.Show("One or both text fields are missing or empty.", "Error", MessageBoxButton.OK,
						MessageBoxImage.Warning);
				}
			}
		}

		private static string FindKeyInIndexArea(string path, int key)
		{
			int blockNumber = 0;

			while (true)
			{
				string indexFilePath = $"{path}{blockNumber}.txt";

				if (!File.Exists(indexFilePath))
				{
					return null;
				}

				string[] lines = File.ReadAllLines(indexFilePath);

				if (lines.Length == 0)
				{
					return null;
				}

				int lastRecordKey = int.Parse(lines[lines.Length - 1].Split(',')[0]);

				if (key > lastRecordKey)
				{
					blockNumber++;
					continue;
				}

				foreach (string line in lines)
				{
					if (int.TryParse(line.Split(',')[0], out int existingKey) && existingKey == key)
					{
						return line;
					}
				}

				return null;
			}
		}

		private void GenerateRecordsOnClick(object sender, RoutedEventArgs e) =>
			CreateInput("Generate", GenerateRecords, 2);

		private void GenerateRecords(object sender, RoutedEventArgs e)
		{
			TextBox keyBox = (TextBox)FindName("text0");

			if (keyBox != null && !string.IsNullOrWhiteSpace(keyBox.Text))
			{
				if (int.TryParse(keyBox.Text, out int maxKey))
				{
					for (int i = 1; i <= maxKey; i++)
					{
						if (!Files.AddToIndexArea("block", i, GenerateString(10)))
						{
							MessageBox.Show("This key exists already! Please enter a valid integer.", "Error", MessageBoxButton.OK,
								MessageBoxImage.Error);
						}
					}
				}
				else
				{
					MessageBox.Show("Invalid key! Please enter a valid integer.", "Error", MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			}
			else
			{
				MessageBox.Show("One or both text fields are missing or empty.", "Error", MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}
		}

		private string GenerateString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			Random random = new Random();

			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}

		private void EditRecordOnClick(object sender, RoutedEventArgs e) =>
			CreateInput("Edit", EditRecord, 3);

		private void EditRecord(object sender, RoutedEventArgs e)
		{
			TextBox keyBox = (TextBox)FindName("text0");
			TextBox newValueBox = (TextBox)FindName("text1");
			if (keyBox != null && !string.IsNullOrWhiteSpace(keyBox.Text) &&
			    newValueBox != null && !string.IsNullOrWhiteSpace(newValueBox.Text))
			{
				if (int.TryParse(keyBox.Text, out int key))
				{
					string foundRecord = FindKeyInIndexArea("block", key);
					if (foundRecord == null)
					{
						MessageBox.Show("This key doesn't exist!", "Error", MessageBoxButton.OK,
							MessageBoxImage.Error);
					}
					else
					{
						string newValue = newValueBox.Text;
						bool isEditedInIndexArea = EditKeyInIndexArea("block", key, newValue);
						bool isEditedInDataFile = EditKeyInDataFile("dataFile.txt", key, newValue);

						if (isEditedInIndexArea && isEditedInDataFile)
						{
							MessageBox.Show("Record successfully edited.", "Success", MessageBoxButton.OK,
								MessageBoxImage.Information);

							keyBox.Clear();
							newValueBox.Clear();
						}
						else
						{
							MessageBox.Show("Failed to edit the record. Please try again.", "Error", MessageBoxButton.OK,
								MessageBoxImage.Error);
						}
					}
				}
				else
				{
					MessageBox.Show("Invalid key! Please enter a valid integer.", "Error", MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			}
			else
			{
				MessageBox.Show("Text fields are missing or empty.", "Error", MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}
		}

		private bool EditKeyInIndexArea(string path, int key, string newValue)
		{
			int blockNumber = 0;

			while (true)
			{
				string indexFilePath = $"{path}{blockNumber}.txt";

				if (!File.Exists(indexFilePath))
				{
					return false;
				}

				string[] lines = File.ReadAllLines(indexFilePath);

				for (int i = 0; i < lines.Length; i++)
				{
					string[] parts = lines[i].Split(',');
					if (int.TryParse(parts[0], out int currentKey) && currentKey == key)
					{
						parts[1] = newValue;
						lines[i] = string.Join(", ", parts);

						File.WriteAllLines(indexFilePath, lines);
						return true;
					}
				}

				blockNumber++;
			}
		}

		private bool EditKeyInDataFile(string dataFilePath, int key, string newValue)
		{
			if (!File.Exists(dataFilePath))
			{
				return false;
			}

			string[] lines = File.ReadAllLines(dataFilePath);

			for (int i = 0; i < lines.Length; i++)
			{
				string[] parts = lines[i].Split(',');
				if (int.TryParse(parts[1], out int currentKey) && currentKey == key)
				{
					parts[2] = newValue;
					lines[i] = string.Join(", ", parts);

					File.WriteAllLines(dataFilePath, lines);
					return true;
				}
			}

			return false;
		}

		private void DeleteRecordOnClick(object sender, RoutedEventArgs e) =>
			CreateInput("Delete", DeleteRecord, 2);

		private void DeleteRecord(object sender, RoutedEventArgs e)
		{
			TextBox keyBox = (TextBox)FindName("text0");
			if (keyBox != null && !string.IsNullOrWhiteSpace(keyBox.Text))
			{
				if (int.TryParse(keyBox.Text, out int key))
				{
					string foundRecord = FindKeyInIndexArea("block", key);
					if (foundRecord == null)
					{
						MessageBox.Show("This key doesn't exist!", "Error", MessageBoxButton.OK,
							MessageBoxImage.Error);
					}
					else
					{
						bool isDeleted = DeleteKeyFromIndexArea("block", key);

						if (isDeleted)
						{
							UpdateDataFile(key);

							MessageBox.Show("Record successfully deleted and updated.", "Success", MessageBoxButton.OK,
								MessageBoxImage.Information);

							keyBox.Clear();
						}
						else
						{
							MessageBox.Show("Failed to delete the record. Please try again.", "Error", MessageBoxButton.OK,
								MessageBoxImage.Error);
						}
					}
				}
				else
				{
					MessageBox.Show("Invalid key! Please enter a valid integer.", "Error", MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			}
			else
			{
				MessageBox.Show("Text field is missing or empty.", "Error", MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}
		}

		private void DeleteAllRecordsOnClick(object sender, RoutedEventArgs e)
		{
			if (Input.Children.Count != 0)
			{
				if (Input.Children[0] is Grid grid)
				{
					for (int i = 0; i < grid.Children.Count; i++)
					{
						if (grid.Children[i] is FrameworkElement child && FindName(child.Name) != null)
						{
							this.UnregisterName(child.Name);
						}
					}
				}
			}
			
			int count = 0;
			if (!File.Exists($"block{count}.txt") || File.ReadAllLines("dataFile.txt").Length == 0)
			{
				MessageBox.Show("You have no any records!", "Error", MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			else
			{
				while (File.Exists($"block{count}.txt"))
				{
					File.Delete($"block{count}.txt");
					count++;
				}
				
				File.WriteAllText("dataFile.txt", "");
			}
		}
		
		private bool DeleteKeyFromIndexArea(string path, int key)
		{
			int blockNumber = 0;

			while (true)
			{
				string indexFilePath = $"{path}{blockNumber}.txt";

				if (!File.Exists(indexFilePath))
				{
					return false;
				}

				string[] lines = File.ReadAllLines(indexFilePath);

				for (int i = 0; i < lines.Length; i++)
				{
					int currentKey = int.Parse(lines[i].Split(',')[0]);
					if (currentKey == key)
					{
						List<string> updatedLines = new List<string>(lines);
						updatedLines.RemoveAt(i);

						File.WriteAllLines(indexFilePath, updatedLines);

						return true;
					}
				}

				blockNumber++;
			}
		}

		private void UpdateDataFile(int key)
		{
			string dataFilePath = "dataFile.txt";

			if (!File.Exists(dataFilePath))
			{
				MessageBox.Show("Data file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			string[] lines = File.ReadAllLines(dataFilePath);
			for (int i = 0; i < lines.Length; i++)
			{
				string[] parts = lines[i].Split(',');
				if (int.TryParse(parts[1], out int currentKey) && currentKey == key)
				{
					parts[0] = "True";
					lines[i] = string.Join(", ", parts);
					break;
				}
			}

			File.WriteAllLines(dataFilePath, lines);
		}

		private void CreateInput(string buttonName, RoutedEventHandler eventHandler, int amountOfItems)
		{
			if (Input.Children.Count != 0)
			{
				if (Input.Children[0] is Grid grid)
				{
					for (int i = 0; i < grid.Children.Count; i++)
					{
						if (grid.Children[i] is FrameworkElement child && FindName(child.Name) != null)
						{
							this.UnregisterName(child.Name);
						}
					}
				}
			}

			Input.Children.Clear();

			Grid input = new Grid();

			for (int i = 0; i < amountOfItems; i++)
				input.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = new GridLength(1, GridUnitType.Star)
				});

			for (int i = 0; i < amountOfItems - 1; i++)
			{
				TextBox key = new TextBox
				{
					Width = 120,
					Height = 20,
					Name = $"text{i}",
					Margin = new Thickness(5)
				};

				this.RegisterName(key.Name, key);
				Grid.SetColumn(key, i);
				input.Children.Add(key);
			}

			Button addBtn = new Button
			{
				Content = $"{buttonName} a record",
				Margin = new Thickness(5),
				Name = "actionBtn",
				Width = 120
			};

			this.RegisterName(addBtn.Name, addBtn);
			addBtn.Click += eventHandler;
			Grid.SetColumn(addBtn, 2);
			input.Children.Add(addBtn);

			Input.Children.Add(input);
		}
	}
}