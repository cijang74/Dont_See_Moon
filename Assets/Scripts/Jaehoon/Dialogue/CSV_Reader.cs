using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions; // 쉼표나 따옴표 처리 위해서 필요하다고 함

public static class CSV_Reader
{
    // 아래 정규식(RegularExpressions)들은 공용 코드라고 하더라고. 퍼왔음
    // 쉼표로 나누기, 하지만 큰따옴표("") 안에 있는 쉼표는 무시
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

    // 줄 바꿈 기호
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

    static char[] TRIM_CHARS = { '\"' }; // 양옆의 \" 문자를 떼어줌

    public static List<Dictionary<string, object>> Read_CSV(string filePath)
    {
        var list = new List<Dictionary<string, object>>();

        // file 경로에서 데이터 읽기
        TextAsset data = Resources.Load(filePath) as TextAsset; // 메모리 상에서 data안에는 "ID,Name,HP,ATK,DEF,Speed,PrefabPath\n1,Warrior,100,20,10,5,Prefabs/Units/Warrior\n2,Mage,70,35,5,6,Prefabs/Units/Mage\n..."처럼 한줄로 저장
        if(data == null)
        {
            Debug.Log($"path:{filePath} 파일을 찾지 못함. null 에러");
            return null;
        }

        // 한줄로 작성된 문자열을 줄바꿈 기호를 기준으로 쪼개서 lines에 저장
        string[] lines = Regex.Split(data.text, LINE_SPLIT_RE); // 메모리 상에서 lines[0]안에는 "ID,Name,HP,ATK,DEF,Speed,PrefabPath", lines[1] 안에는 "1,Warrior,100,20,10,5,Prefabs/Units/Warrior"
        if (lines.Length <= 1)
        {
            Debug.Log($"path:{filePath} 파일 안의 내용이 없음");
            return list;
        }

        string[] header = Regex.Split(lines[0], SPLIT_RE); // 첫째줄(lines[0])은 헤더(컬럼 이름)임

        for (var i = 1; i < lines.Length; i++) // 다음 줄부터는 데이터 읽기(for문)
        {
            // 라인(n번째 줄) 전체가 비어있다면 건너뛰기
            if(string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            // '/' 으로 시작하면 주석이라는 뜻.. NOTE: 기획과 상의하기
            if(lines[i].TrimStart(TRIM_CHARS).StartsWith('/'))
            {
                continue;
            }
            
            string[] values = Regex.Split(lines[i], SPLIT_RE); // 값들 분해: lines[0]안에는 "ID,Name,HP,ATK,DEF,Speed,PrefabPath" -> values[0] 안에는 "1", values[1] 안에는 "Warrior"...

            // 라인(n번째 줄)중 0번째 인덱스 값이 비어있거나 길이가 0이라면 정상적으로 작성 안된것으로 판단, 건너뛰기
            if (values.Length == 0 || values[0] == "")
            {
                continue;
            }

            var entry = new Dictionary<string, object>(); // 하나의 row 에서 딕셔너리 생성

            // 딕셔너리 매핑 과정
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];

                // CSV 특정 상 데이터 앞 뒤로 "나 공백, \가 붙는 경우가 있음. 이를 제거하고 텍스트 안에 있는 <br>을 실제 줄바꿈 기호로 바꿔줌.
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "").Replace("<br>", "\n");

                //if (!string.IsNullOrEmpty(value) && value.StartsWith("/")) value = ""; <- csv 맨 뒤에 주석 달려고 넣은거, 근데 for문 조건에서 컷 가능하니까 일단 주석처리 했음

                // object 타입,, var는 컴파일러가 알아서 타입을 추론, object 는 모든 타입의 부모
                object finalvalue = value;
                int n;
                float f;

                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }

                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                
                // 데이터가 int면 int로 저장, 아니면 string으로 저장한다는 뜻. float는 우리 기획에 없지만..
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        Resources.UnloadAsset(data); // 메모리 누수 방지. Resources.Load 쓰는 애들 다 이거 찾아서 해줘야함; 큰일났다 이제
        //Resources.UnloadUnusedAssets(); 이거 씬 넘어갈 때 써주면 된다고는 하는데, 어디서 참조중이면 이걸로도 해결 안됨
        return list;
    }
}
