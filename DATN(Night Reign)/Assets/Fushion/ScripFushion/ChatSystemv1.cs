using Fusion;
using TMPro;
using UnityEngine;

public class ChatSystemv1 : NetworkBehaviour
{
    private GameObject Chat;         // Object chứa giao diện chat
    private TextMeshProUGUI TextMess; // Hiển thị tin nhắn chính
    private TextMeshProUGUI MessHis;  // Hiển thị lịch sử tin nhắn
    private TMP_InputField Chattext; // Input field để nhập tin nhắn
    private GameObject Chatbutton;   // Nút gửi tin nhắn

    private bool isChatActive = false; // Trạng thái bật/tắt chat
    private DuyPlayerMovement playerMovement; // Tham chiếu đến script điều khiển di chuyển
    private CamFPS camFPS;                 // Tham chiếu đến script camera FPS

    public override void Spawned()
    {
        // Tìm tất cả các thành phần trong scene
        Chat = GameObject.Find("Chat");
        TextMess = GameObject.Find("TextMess").GetComponent<TextMeshProUGUI>();
        MessHis = GameObject.Find("MessHis").GetComponent<TextMeshProUGUI>();
        Chattext = GameObject.Find("Chattext").GetComponent<TMP_InputField>();
        Chatbutton = GameObject.Find("Chatbutton");

        // Gán sự kiện click cho Chatbutton
        Chatbutton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ButtonSendClick);

        // Tìm script điều khiển di chuyển của người chơi
        playerMovement = GetComponent<DuyPlayerMovement>();

        // Tìm script CamFPS
        camFPS = GetComponent<CamFPS>();

        SetChatActive(false); // Tắt chat khi bắt đầu vào game
    }

    private void Update()
    {
        // Kiểm tra nếu người chơi nhấn phím Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleChat(); // Bật/tắt giao diện chat
        }
        if (isChatActive && Input.GetKeyDown(KeyCode.Return))
        {
            ButtonSendClick(); // Gửi tin nhắn nếu chat đang bật
        }
    }

    public void ButtonSendClick()
    {
        if (!isChatActive) return; // Không gửi nếu chat bị tắt

        var message = Chattext.text;
        if (string.IsNullOrWhiteSpace(message)) return;

        var id = Runner.LocalPlayer.PlayerId;
        var text = $"Player {id}: {message}";
        Rpcmethod(text);
        Chattext.text = ""; // Xóa nội dung khung chat

        // Kích hoạt lại khung chat để tiếp tục nhập
        Chattext.ActivateInputField();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpcmethod(string message)
    {
        TextMess.text += message + "\n";
        MessHis.text += message + "\n"; // Lưu vào lịch sử
    }

    public void ToggleChat()
    {
        // Đổi trạng thái chat
        isChatActive = !isChatActive;
        SetChatActive(isChatActive);

        // Ngăn/cho phép di chuyển
        if (playerMovement != null)
        {
            playerMovement.canMove = !isChatActive;
        }

        // Báo cho CamFPS biết trạng thái chat
        if (camFPS != null)
        {
            camFPS.SetChatState(isChatActive);
        }
    }

    private void SetChatActive(bool isActive)
    {
        // Bật/tắt toàn bộ giao diện chat
        Chat.SetActive(isActive);

        // Đặt con trỏ vào khung chat khi chat bật
        if (isActive)
        {
            Chattext.ActivateInputField(); // Kích hoạt khung nhập tin nhắn
        }
    }
}
