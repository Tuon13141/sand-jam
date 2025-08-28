# Cấu trúc các thư mục #

Trong 1 project sẽ có các folder chung sau:

* Plugins (Lưu các Package chung đắcj biệt như Dotween, UniTask,...)
* Project (Lưu các folder về scene, asset, script của project)
* Resources (lưu data, sfx, bgm, voice và một số thứ khác cần load nhanh trực tiếp trong app) 
* ThirdParties (Dùng để lưu các package import từ bên ngoài) Việc lưu vào 1 folder tổng giúp dễ quản lý, dễ xóa nếu ko dùng.

<p align="center">
  <img width="500px" src="https://i.ibb.co/TMh2YCWd/image.png" alt="Demo">
</p>

Đa phần developer sẽ làm việc trong thư mục Project, trong đó có các folder con phân tầng như sau:

* [Project/2D Textures] - lưu trữ tất cả các texture 2D có trong game.
* [Project/3D Models] - lưu trữ tất cả những gì liên quan đến model gồm Animation, Materials, Textures export và import cùng model (đối với game 3D)
* [Project/Animations] - lưu trữ các animation tự tạo, dùng chung trong toàn bộ project. Với các animation dành riêng cho model, object nào đó thì không lưu ở đây.
* [Project/Fonts] - tất cả những gì liên quan đến font sau khi import vào project cùng với Font Asset của Text Mesh Pro.
* [Project/Prefabs] - nơi tạo và lưu các prefab dùng chung trong toàn bộ project.
* [Project/Scenes] - nơi lưu trữ các scene.
* [Project/Scripts] - nơi lưu trữ các scipts cho hệ thống game.

<p align="center">
  <img width="500px" src="https://i.ibb.co/v46F9BG7/Screenshot-2025-05-12-162304.png" alt="Demo">
</p>

# Các Package đã có sắn trong Template #

Trong Plugins:

* [Dotween Pro](https://dotween.demigiant.com/documentation.php)
* [UniTask](https://github.com/Cysharp/UniTask)

Trong Project/Scripts/Common:

* AnimationSupport - Hỗ trợ debug Animation, Animator, show từng frame + Các hàm hỗ trợ Play/PlayAsync/PlayReverse với speed, duration, SetupFirstFrameAnimation, SetupLastFrameAnimation,...
* GizmosExtend - Hỗ trợ debug nhanh gizmos như DrawSquare, DrawLabel, DrawLine,... + singleton để gọi Draw ở mọi vi trí code.
* MatrixType - Hỗ trợ Serialize Array2D với nhiều loại data khác nhau: number, string, boolean, object,... dễ dàng setup. 
* Pooling - pooling object dùng cho PrefabManager.
* TupleSerialize - Hỗ trợ Serialize tuple.
* Ngoài ra sẽ có thêm các tool hỗ trợ thêm trong Editor như: Merge Mesh, Auto Grid Snap, Detect Missing Script, CameraScene Tool, Scene Switcher,...

Trong ThirdParties:

* [Particle Image](https://assetstore.unity.com/packages/tools/gui/ui-particle-image-235001) - hỗ trợ particle trên UI nhẹ, nhiều tính năng
* [CameraScaler](https://assetstore.unity.com/packages/tools/camera/camera-scaler-228305) - Fill size camera theo kích thước màn hình
* [Color Toony Pro](https://assetstore.unity.com/packages/vfx/shaders/toony-colors-pro-2-8105) - hỗ trợ shader toony.
* [PlayerPrefsEditor](https://github.com/sabresaurus/PlayerPrefsEditor) - hỗ trợ đọc, sửa nhanh playerpref
* [PlayModeComponentSave](https://assetstore.unity.com/packages/tools/utilities/play-mode-saver-104836) - hỗ trợ save component trên scene khi chỉnh sửa ở PlayTime.
* [ScreenshotUtility](https://assetstore.unity.com/packages/tools/utilities/screenshot-utility-177723) - hỗ trợ chụp màn hình gameplay, scene độ phân giải cao.
* [Unity-Logs-Viewer](https://assetstore.unity.com/packages/tools/integration/log-viewer-12047) - hỗ trợ debug log không che màn hình, nhiều chức năng, mở log bằng cách vẽ 10 vòng tròn trên màn hình.