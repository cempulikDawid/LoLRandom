using System;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using System.Windows.Controls;

namespace LoLRandom
{
    public partial class App : Application
    {
        public static string championsURL = "https://leagueoflegends.fandom.com/wiki/List_of_champions_by_draft_position";
        public static string championsStatsURL = "http://ddragon.leagueoflegends.com/cdn/11.24.1/data/en_US/champion.json";
        public static string imagesURL = "http://ddragon.leagueoflegends.com/cdn/img/champion/loading/";

        public static string imagesFilePath = "images\\";

        HttpClient client;
        HtmlWeb web;
        public static Task champNamesSync;
        public static Task champImagesSync;

        public static ChampList champList = new ChampList();

        public App()
        {
            client = new HttpClient();
            web = new HtmlWeb();

            LoadChampList();
            champNamesSync = SynchronizeChampList();
            champImagesSync = SynchronizeChampImages();
        }

        private void SaveChampList()
        {
            var writerOptions = new JsonWriterOptions
            {
                Indented = true
            };

            var fileStream = File.Create("champList.json");
            Utf8JsonWriter writer = new Utf8JsonWriter(fileStream, writerOptions);

            writer.WriteStartObject();
            writer.WriteStartObject("champions");
            foreach (Champion champ in champList.champions)
            {
                writer.WriteStartObject(champ.id);
                writer.WritePropertyName("name");
                writer.WriteStringValue(champ.name);

                writer.WriteStartArray("positions");
                foreach (string position in champ.positions)
                {
                    writer.WriteStringValue(position);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
            writer.WriteEndObject();

            writer.Flush();
            fileStream.Close();
        }

        private void LoadChampList()
        {
            if (!File.Exists("champList.json")) return;
            ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes("champList.json");

            champList = new ChampList();
            Champion champion = new Champion();

            var options = new JsonReaderOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };

            Utf8JsonReader reader = new Utf8JsonReader(jsonReadOnlySpan, options);

            while (reader.Read())
            {
                JsonTokenType type = reader.TokenType;
                int depth = reader.CurrentDepth;

                if (depth == 2)
                {
                    if (type == JsonTokenType.PropertyName)
                    {
                        champion.id = reader.GetString();
                    }
                }
                else if (depth == 3)
                {
                    if (type == JsonTokenType.PropertyName)
                    {
                        if (reader.GetString() == "name")
                        {
                            reader.Read();
                            champion.name = reader.GetString();
                        }
                        else if(reader.GetString() == "positions")
                        {
                            reader.Read();
                            reader.Read();
                            while(reader.TokenType != JsonTokenType.EndArray)
                            {
                                champion.positions.Add(reader.GetString());
                                reader.Read();
                            }

                            champList.champions.Add(champion);
                            foreach (string position in champion.positions)
                            {
                                if (position == "top") champList.topChampions.Add(champion);
                                else if (position == "jungle") champList.jungleChampions.Add(champion);
                                else if (position == "mid") champList.midChampions.Add(champion);
                                else if (position == "bot") champList.botChampions.Add(champion);
                                else if (position == "support") champList.supportChampions.Add(champion);
                            }
                            champion = new Champion();
                        }
                    }
                }
            }
        }

        private async Task SynchronizeChampList()
        {
            string response = await client.GetStringAsync(championsStatsURL);
            JsonElement data = JsonDocument.Parse(response).RootElement.GetProperty("data");


            HtmlDocument htmlDoc = await web.LoadFromWebAsync(championsURL);
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//table[@class='article-table sortable']/tbody");

            List<HtmlNode> rows = new List<HtmlNode>(node.Elements("tr"));

            for (int row = 1; row < rows.Count; row++)
            {
                HtmlNode rowNode = rows[row];

                Champion champion = new Champion();

                List<HtmlNode> cols = new List<HtmlNode>(rowNode.Elements("td"));

                for (int col = 0; col < cols.Count; col++)
                {
                    HtmlNode colNode = cols[col];

                    if (col == 0)
                    {
                        string name = colNode.Descendants("a").Last().InnerText;
                        name = name.Replace("&amp;", "&");
                        champion.name = name;

                        foreach (JsonProperty champ in data.EnumerateObject())
                        {
                            JsonElement champp = champ.Value;
                            if (name == champp.GetProperty("name").GetString())
                                champion.id = champp.GetProperty("id").GetString();
                        }
                    }
                    else
                    {
                        string attribute = colNode.GetAttributeValue("data-sort-value", "0");
                        if (attribute == "1" || attribute == "2" || attribute == "3")
                        {
                            switch (col)
                            {
                                case 1:
                                    champion.positions.Add("top");
                                    champList.topChampions.Add(champion);
                                    break;
                                case 2:
                                    champion.positions.Add("jungle");
                                    champList.jungleChampions.Add(champion);
                                    break;
                                case 3:
                                    champion.positions.Add("mid");
                                    champList.midChampions.Add(champion);
                                    break;
                                case 4:
                                    champion.positions.Add("bot");
                                    champList.botChampions.Add(champion);
                                    break;
                                case 5:
                                    champion.positions.Add("support");
                                    champList.supportChampions.Add(champion);
                                    break;
                            }
                        }
                    }
                }

                champList.champions.Add(champion);
            }

            SaveChampList();
        }

        private async Task SynchronizeChampImages()
        {
            await champNamesSync;
            Directory.CreateDirectory(imagesFilePath);
            foreach (Champion champion in champList.champions)
            {
                if (File.Exists(imagesFilePath + champion.id + ".jpg")) continue;
                HttpResponseMessage response = await client.GetAsync(imagesURL + champion.id + "_0.jpg");
                var contentStream = await response.Content.ReadAsStreamAsync();
                var fileStream = File.Create(imagesFilePath + champion.id + ".jpg");
                contentStream.Seek(0, SeekOrigin.Begin);
                await contentStream.CopyToAsync(fileStream);
                await contentStream.FlushAsync();
                contentStream.Close();
                fileStream.Close();
            }
        }
    }
}
