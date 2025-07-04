//// File: LocalizedString.cs
//using UnityEngine;

//// Giả sử bạn có một trình quản lý localization tĩnh như thế này
//// Ví dụ: LocalizationManager.GetTranslation("quest_01_name") -> "Thu thập Gỗ"
//namespace npc.localization
//{
//    public static class LocalizationManager
//    {
//        // *** THAY THẾ HÀM NÀY BẰNG HỆ THỐNG LOCALIZATION THỰC TẾ CỦA BẠN ***
//        public static string GetTranslation(string key)
//        {
//            // Đây chỉ là ví dụ, bạn cần gọi đến hệ thống localization của mình ở đây.
//            // Ví dụ: return I2.Loc.LocalizationManager.GetTranslation(key);
//            // Hoặc: return Lean.Localization.LeanLocalization.GetTranslationText(key);
//            if (string.IsNullOrEmpty(key)) return "INVALID_KEY";
//            return $"[{key}]"; // Trả về key nếu không tìm thấy để dễ debug
//        }
//    }


//    [System.Serializable]
//    public class LocalizedString
//    {
//        public string key;

//        public LocalizedString(string key)
//        {
//            this.key = key;
//        }

//        // Đây là phần quan trọng nhất:
//        // Tự động chuyển đổi một đối tượng LocalizedString thành một string
//        // khi code cần một string.
//        public static implicit operator string(LocalizedString localizedString)
//        {
//            if (localizedString == null || string.IsNullOrEmpty(localizedString.key))
//            {
//                return "";
//            }
//            // Tự động gọi đến hệ thống dịch thuật của bạn
//            return LocalizationManager.GetTranslation(localizedString.key);
//        }
//    }
//}