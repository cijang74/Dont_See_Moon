using System.Collections.Generic;
using UnityEngine;

// 뉴스 한 개 (제목 + 작성 날짜 + 본문)
public class NewsItem
{
    public string newsId;
    public string title;                              // 뉴스 제목
    public string date;                               // 표시용 작성 날짜 (예: "2024.03.15")
    public int triggerDay;                            // 등장 날짜 (게임 일차)
    public List<string> body = new List<string>();    // 본문 문단들(순서대로)
}

// 뉴스 런타임 상태(배지/읽음 여부)
public class NewsRuntime
{
    public NewsItem data;
    public bool hasUnread; // 빨간 배지 표시 여부
    public bool read;      // 읽음 여부
}

// NewsRoom.csv / NewsChats.csv → NewsItem 으로 변환. 구분자는 파이프(|).
public static class NewsCsvLoader
{
    private const char Delimiter = '|';
    private static readonly string DelimiterStr = Delimiter.ToString();

    // NewsRoom  : NewsId|Title|Date|Day
    // NewsChats : NewsId|Text   (Text는 항상 마지막 칸, 한 줄 = 한 문단)
    public static Dictionary<string, NewsItem> Load(TextAsset roomCsv, TextAsset chatsCsv)
    {
        var items = new Dictionary<string, NewsItem>();
        if (roomCsv == null || chatsCsv == null)
        {
            Debug.LogError("NewsCsvLoader: CSV TextAsset이 할당되지 않았습니다.");
            return items;
        }

        // 1) 뉴스 목록
        var roomRows = ParseLines(roomCsv.text);
        for (int r = 1; r < roomRows.Count; r++)
        {
            var row = roomRows[r];
            if (row.Length < 4 || string.IsNullOrWhiteSpace(row[0])) continue;

            int.TryParse(row[3].Trim(), out int day);
            var item = new NewsItem
            {
                newsId = row[0].Trim(),
                title = row[1].Trim(),
                date = row[2].Trim(),
                triggerDay = day
            };
            items[item.newsId] = item;
        }

        // 2) 본문
        var bodyRows = ParseLines(chatsCsv.text);
        for (int r = 1; r < bodyRows.Count; r++)
        {
            var row = bodyRows[r];
            if (row.Length < 2 || string.IsNullOrWhiteSpace(row[0])) continue;

            string id = row[0].Trim();
            if (!items.TryGetValue(id, out var item)) continue;
            item.body.Add(JoinFrom(row, 1)); // 2번째 칸부터 끝까지 = 본문
        }

        return items;
    }

    static string JoinFrom(string[] arr, int start)
    {
        if (arr.Length <= start) return "";
        return string.Join(DelimiterStr, arr, start, arr.Length - start);
    }

    static List<string[]> ParseLines(string text)
    {
        var rows = new List<string[]>();
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            rows.Add(line.Split(Delimiter));
        }
        return rows;
    }
}