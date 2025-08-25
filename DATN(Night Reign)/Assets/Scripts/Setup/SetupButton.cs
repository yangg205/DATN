using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Localization.Settings;

public class SetupButton : MonoBehaviour
{
    public List<ButtonSetup> ButtonSetups;

    private async void Start()
    {
        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            Debug.Log("Đang chờ khởi tạo LocalizationSettings...");
            await LocalizationSettings.InitializationOperation.Task;
        }

        Debug.Log($"Current locale: {LocalizationSettings.SelectedLocale?.Identifier.Code ?? "None"}");
        await UpdateAllButtons();
    }

    private async Task UpdateAllButtons()
    {
        if (ButtonSetups == null || ButtonSetups.Count == 0)
        {
            Debug.LogWarning("Mảng ButtonSetups trống hoặc không được gán!");
            return;
        }

        var tasks = new List<Task>();
        foreach (var setup in ButtonSetups)
        {
            if (setup.GameObject != null && !string.IsNullOrEmpty(setup.Key))
            {
                Debug.Log($"Áp dụng localization cho GameObject: {setup.GameObject.name}, Key: {setup.Key}");
                tasks.Add(setup.GameObject.SetLocalizationKey(setup.Key));
            }
            else
            {
                Debug.LogWarning($"GameObject hoặc Key không hợp lệ: GameObject={setup.GameObject?.name}, Key={setup.Key}");
            }
        }

        await Task.WhenAll(tasks);
        Debug.Log("Hoàn tất cập nhật localization cho tất cả các nút.");
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += OnLanguageChangedHandler;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= OnLanguageChangedHandler;
    }

    private async void OnLanguageChangedHandler()
    {
        Debug.Log("Ngôn ngữ đã thay đổi, đang làm mới các nút...");
        await UpdateAllButtons();
    }

    [System.Serializable]
    public class ButtonSetup
    {
        public GameObject GameObject;
        public string Key;
    }
}
