//***************************
// 파일명: PersistentObjectSpawner.cs
// 작성자: 김재훈
// 작성일: 2026.03.19
// 내용: 싱글턴으로 관리할 오브젝트들을 자식으로 가진 persistentObjectPrefab를 참조받아 싱글턴으로 생성
//***************************

using System.Collections.Generic;
using UnityEngine;

public class PersistentObjectSpawner : MonoBehaviour
{
    // 툴팁과 지속성 객체 프리펩 생성
    [SerializeField] GameObject persistentObjectPrefab = null;
    public static GameObject PersistentObjectPrefab { get; private set; } //2026. 05. 18. 강문석. 계정 삭제를 위해 추가
    // 중복 생성을 막기 위한 static bool 변수
    static bool hasSpawned = false;

    private void Awake()
    {
        // 이미 생성되었으면 생성X
        if (hasSpawned)
        {
            return;
        }

        // 위에서 return이 안됬으면 생성해주기
        SpawnPersistentObjects();
        hasSpawned = true;
    }

    // 인스턴스화 한 뒤, 삭제되는 것 막기
    private void SpawnPersistentObjects()
    {
        GameObject persistentObject = Instantiate(persistentObjectPrefab);
        DontDestroyOnLoad(persistentObject);
        PersistentObjectPrefab = persistentObject;
    }
    public static void ResetSpawner() //2026. 05. 18. 강문석. 계정 삭제를 위해 추가
    {
        hasSpawned = false;
    }
}
