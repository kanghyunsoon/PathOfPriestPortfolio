using UnityEngine;
using UnityEngine.UI;

public class Mastery_Button : MonoBehaviour
{
    public GameObject SelfMask;
    public Status_Holder Status;
    public float StatusValue;
    public GameObject Left_Next, Right_Next, Left_Line, Right_Line;
    public bool Twin;
    public int Level;
    public int Id;
    public int[] PrerequisiteIds;
    public double Gold;
    // 일정 재화를 소모시킬지?

    private bool isUnlocked = false;

    private void Start()
    {
        string key = $"mastery_{Id}";
        int unlocked = PlayerPrefs.GetInt(key, 0);

        if (unlocked == 1)
        {
            isUnlocked = true;
            if (SelfMask) SelfMask.GetComponent<Image>().color = new Color(0.0f, 0.9725f, 1.0f, 1.0f); // 마스터리 활성화됨
            if (Left_Line) Left_Line.SetActive(true);
            if (Right_Line) Right_Line.SetActive(true);
            if (Left_Next) Left_Next.GetComponent<Image>().color = Color.white;
            if (Right_Next) Right_Next.GetComponent<Image>().color = Color.white;
        }
        else
        {
            if (SelfMask) SelfMask.SetActive(true); // 잠김 상태
        }
    }

    public void MasteryButton()
    {
        if (isUnlocked)
        {
            return;
        }

        if (Level > Data_Manager.m_Data.Level)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NotEnoughLevel"].Get_Data(), Color.red);
            return;

        }


        if (PrerequisiteIds != null && PrerequisiteIds.Length > 0)
        {
            foreach (int preId in PrerequisiteIds)
            {
                string prerequisiteKey = $"mastery_{preId}";
                int unlocked = PlayerPrefs.GetInt(prerequisiteKey, 0);

                if (unlocked == 0) // 하나라도 잠겨있으면
                {
                    Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NeedPreviousMastery"].Get_Data(), Color.red);
                    return;
                }
            }
        }

        // 마지막에는 라인도 넣어주지않기
        if (Left_Next == null && Right_Next == null)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["CompleteTraining"].Get_Data(), Color.red);
        }

        if(Data_Manager.m_Data.Money < Gold)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NotEnoughTrainingCurrency"].Get_Data(), Color.red);
            return;
        }


        Data_Manager.m_Data.Money -= Gold;

        string key = $"mastery_{Id}";
        PlayerPrefs.SetInt(key, 1); // 해금 처리
        PlayerPrefs.Save();

        //  마스터리 합산
        if (Base_Manager.Data.m_Data_Mastery.ContainsKey(Status))
        {
            Base_Manager.Data.m_Data_Mastery[Status] += StatusValue;
        }
        else
        {
            Base_Manager.Data.m_Data_Mastery.Add(Status, StatusValue);
        }

        // 두개가 아니라면 무조건 왼쪽으로 합시다
        if (Twin)
        {
            TryActivateNext(Left_Next);
            TryActivateNext(Right_Next);
        }
        else
        {
            TryActivateNext(Left_Next);
        }

        if (Left_Line) Left_Line.SetActive(true);
        if (Right_Line) Right_Line.SetActive(true);

        if (SelfMask) SelfMask.GetComponent<Image>().color = new Color(0.0f, 0.9725f, 1.0f, 1.0f); ;
        Main_UI.instance.TextCheck();

    }
    private void TryActivateNext(GameObject nextObj)
    {
        if (nextObj == null) return;

        // 상위(Parent)에서 Mastery_Button 스크립트를 찾는다
        Mastery_Button nextMastery = nextObj.GetComponentInParent<Mastery_Button>();
        if (nextMastery == null) return;

        // 그 마스터리 버튼의 PrerequisiteIds를 전부 검사
        if (nextMastery.PrerequisiteIds != null && nextMastery.PrerequisiteIds.Length > 0)
        {
            foreach (int preId in nextMastery.PrerequisiteIds)
            {
                string prerequisiteKey = $"mastery_{preId}";
                int unlocked = PlayerPrefs.GetInt(prerequisiteKey, 0);

                if (unlocked == 0)
                {
                    // 하나라도 잠겨있으면 색칠 안 함
                    return;
                }
            }
        }

        // 모두 열렸으면 하얀색으로
        nextObj.GetComponent<Image>().color = Color.white;
    }

}
