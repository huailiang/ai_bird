using UnityEngine;
using System.Collections;

public class Notification : MonoBehaviour
{
#if UNITY_IPHONE
    //本地推送
    public static void NotificationMessage(string message, int hour, bool isRepeatDay)
    {
        int year = System.DateTime.Now.Year;
        int month = System.DateTime.Now.Month;
        int day = System.DateTime.Now.Day;
        System.DateTime newDate = new System.DateTime(year, month, day, hour, 0, 0);
        NotificationMessage(message, newDate, isRepeatDay);
    }


    //本地推送 你可以传入一个固定的推送时间
    public static void NotificationMessage(string message, System.DateTime newDate, bool isRepeatDay)
    {
        //推送时间需要大于当前时间
        if (newDate > System.DateTime.Now)
        {
            LocalNotification localNotification = new LocalNotification();
            localNotification.fireDate = newDate;
            localNotification.alertBody = message;
            localNotification.applicationIconBadgeNumber = 1;
            localNotification.hasAction = true;
            if (isRepeatDay)
            {
                //是否每天定期循环
                localNotification.repeatCalendar = CalendarIdentifier.ChineseCalendar;
                localNotification.repeatInterval = CalendarUnit.Day;
            }
            localNotification.soundName = LocalNotification.defaultSoundName;
            NotificationServices.ScheduleLocalNotification(localNotification);
        }
    }

    void Awake()
    {
        //第一次进入游戏的时候清空，有可能用户自己把游戏冲后台杀死，这里强制清空
        CleanNotification();
    }

    void OnApplicationPause(bool paused)
    {
        //程序进入后台时
        if (paused)
        {
            //10秒后发送
            NotificationMessage("3DBird 更新了，小伙伴们快来受虐吧！", System.DateTime.Now.AddSeconds(120), false);
            //每天中午12点推送
			NotificationMessage("3DBird 更新了，小伙伴们中午下班了快来受虐吧！", 12, true);
        }
        else
        {
            //程序从后台进入前台时
            CleanNotification();
        }
    }


    //清空所有本地消息
    void CleanNotification()
    {
        LocalNotification l = new LocalNotification();
        l.applicationIconBadgeNumber = -1;
        NotificationServices.PresentLocalNotificationNow(l);
        NotificationServices.CancelAllLocalNotifications();
        NotificationServices.ClearLocalNotifications();
    }
#endif
}