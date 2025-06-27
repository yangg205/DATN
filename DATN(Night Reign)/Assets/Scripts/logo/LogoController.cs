using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Cần thiết cho Coroutine

public class LogoController : MonoBehaviour
{
    public string nextSceneName = "Auth"; // Tên scene đích
    public Animator logoAnimator;        // Kéo Animator của bạn vào đây trong Inspector
    public string logoOutAnimationName = "logoout"; // Tên chính xác của animation clip "logoout"
                                                    // Đảm bảo tên này khớp với tên bạn thấy trong Animator Controller

    private bool animationStarted = false; // Biến cờ để kiểm soát việc bắt đầu lắng nghe

    void Start()
    {
        // Nếu bạn chưa gán Animator trong Inspector, hãy cố gắng tìm nó
        if (logoAnimator == null)
        {
            logoAnimator = GetComponent<Animator>();
            if (logoAnimator == null)
            {
                Debug.LogError("Animator not found on this GameObject. Please assign it or add it to the GameObject.");
                return;
            }
        }

        // Bắt đầu coroutine kiểm tra trạng thái Animator
        StartCoroutine(CheckLogoOutAnimationCompletion());
    }

    IEnumerator CheckLogoOutAnimationCompletion()
    {
        // Chờ một khung hình để Animator khởi tạo
        yield return null;

        // Chờ cho đến khi Animator chuyển sang trạng thái "logoout"
        // Điều này giả định rằng Animator của bạn sẽ tự động chuyển từ Entry -> logo -> idle -> logoout
        while (!logoAnimator.GetCurrentAnimatorStateInfo(0).IsName(logoOutAnimationName))
        {
            yield return null; // Chờ cho đến khi animation "logoout" bắt đầu chạy
        }

        Debug.Log("Animation '" + logoOutAnimationName + "' đã bắt đầu.");

        // Chờ cho đến khi animation "logoout" hoàn tất
        // Sử dụng AnimatorStateInfo để kiểm tra thời gian chuẩn hóa (normalizedTime)
        // normalizedTime >= 1.0f nghĩa là animation đã kết thúc
        while (logoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            // Kiểm tra xem Animator có bị chuyển sang một trạng thái khác không
            if (!logoAnimator.GetCurrentAnimatorStateInfo(0).IsName(logoOutAnimationName))
            {
                // Nếu nó đã chuyển sang Exit hoặc một trạng thái khác, ta coi như đã kết thúc
                Debug.LogWarning("Animator transitioned out of '" + logoOutAnimationName + "' early.");
                break;
            }
            yield return null;
        }

        // Đảm bảo là chúng ta không chuyển scene quá sớm nếu có nhiều animation chạy cùng lúc
        // Tùy chọn: Chờ thêm một chút sau khi normalizedTime đạt 1.0f để đảm bảo mọi thứ đã hoàn tất
        // yield return new WaitForSeconds(0.1f); // Một khoảng chờ nhỏ

        Debug.Log("Animation '" + logoOutAnimationName + "' đã hoàn tất. Chuyển scene...");
        SceneManager.LoadScene(nextSceneName);
    }
}