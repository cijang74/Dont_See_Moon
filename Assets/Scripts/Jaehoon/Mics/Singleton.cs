//***************************
// 파일명: Singleton.cs
// 작성자: 김재훈
// 작성일: 2026.03.19
// 내용: 해당 스크립트 상속받아 사용하게 되면 싱글턴으로 사용할 수 있게 됨
//***************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T> // 제너릭으로 사용할 수 있는 클래스는 반드시 Singleton 클래스를 상속받은 클래스 여야만 함
{
    private static T instance;
    public static T Instance {get { return instance; } } // 상속받은 클래스에서 접근할 수 있도록

    protected virtual void Awake()
    {
        // 해당 인스턴스가 속한 모노비헤이버 클래스에 Awake()할당
        if(instance != null && this.gameObject != null) // 이미 존재하면 인스턴스 삭제
        {
            Destroy(this.gameObject);
        }

        else // 인스턴스가 존재하지 않으면 인스턴스 생성
        {
            instance = (T)this;
        }

        if(!gameObject.transform.parent) // 만약 DontDestroyOnLoad로 설정할 때 게임 오브젝트가 최상위 오브젝트가 아니면 자식만 살아남는 꼴이 되므로 오류 방지
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this) instance = null;  //2026. 05. 18. 강문석. 계정 삭제를 위해 추가
    }
}