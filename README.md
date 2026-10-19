# 1151VR-HW4-414262272-LCH

## 一、專案截圖

### 1. 3D 飛行過關場景與飛行器控制
![3D 飛行器與穿越障礙物](Docs/Screenshots/flight_gameplay.png)

### 2. 順利穿過光圈觸發成功音效與計分
![穿越光圈成功反饋](Docs/Screenshots/pass_ring.png)

### 3. 撞擊障礙物墜毀與失誤重置
![撞擊障礙物失誤反饋](Docs/Screenshots/obstacle_crash.png)

---

## 二、GitHub 連結

- **GitHub Repository**: https://github.com/Johnson-bro-coder/1151VR-HW4-414262272-LCH

---

## 三、YouTube 連結

- **YouTube 演示影片**: [請在此填入個人 YouTube 錄影連結，例：https://youtu.be/xxxxxx]

---

## 四、說明製作流程和相關操作

### 1. Unity 3D 專案建置與資源匯入
- **專案架構**：使用 Unity 6 (6000.3.25f1) 3D Core 模板建立專案。
- **3D 模型資源**：
  - `Airplane.obj`：流線型 3D 飛行器模型（具備機身、掠翼主翼、水平尾翼與垂直尾翼結構）。
  - `RingObstacle.obj`：環狀穿透光圈障礙物（外圈設置撞擊碰撞體，中心圓孔設置 Trigger 穿透觸發區）。
  - `PillarObstacle.obj`：垂直石柱障礙物（配置 Capsule Collider 實體碰撞體）。
- **音效資源 (Audio Clips)**：
  - `Airplane_Fly_Loop.wav`：飛行巡航持續引擎聲，巡航時依速度動態調整 Pitch（音高）。
  - `Obstacle_Crash.wav`：撞擊外框或石柱時觸發的墜毀爆炸破裂音效。
  - `Pass_Ring_Success.wav`：精準穿過光圈內部時觸發的悅耳通關提示和弦音。

### 2. 3D 飛行控制與空氣動力邏輯 (`AirplaneController.cs`)
- **前進推力與速度調節**：
  - 機身維持恆定巡航基礎速度，提供 Shift / Space 油門加速 (Boost) 與 Ctrl 減速 (Brake) 機制。
- **三軸姿態機動控制 (Pitch, Yaw, Roll)**：
  - **俯仰 (Pitch)**：W/S 或 方向鍵上下，調整機頭仰角與俯衝。
  - **偏航 (Yaw)**：A/D 或 方向鍵左右，控制航向水平轉彎。
  - **滾轉 (Roll & Banking)**：Q/E 鍵進行特技側滾；左右轉向時自動帶入空氣動力側傾角（Banking），大幅提升飛行真實感。
  - **水平自動回正 (Auto-Leveling)**：玩家釋放轉向鍵時，系統依阻尼係數自動平滑校正機身至水平平衡姿態。
- **動態音效連動**：
  - 根據當前飛行速度與最大速度比例，即時調整 `AudioSource.pitch`（0.85 ~ 1.45 倍），模擬油門增壓音效。

### 3. 障礙物判定與音效反饋機制
- **穿透式光圈障礙 (`RingObstacle.cs`)**：
  - 採用 `OnTriggerEnter` 偵測飛行器穿入。
  - 成功判定後播放 `Pass_Ring_Success.wav` 音效，光圈顏色由金黃色即時變為翠綠色，並於 HUD 累加通過數量（`passedRings++`）。
- **實體危險障礙物 (`HazardObstacle.cs`)**：
  - 設置於石柱、光圈外框及地面，採用 `OnCollisionEnter`。
  - 撞擊瞬間觸發 `Obstacle_Crash.wav`、中斷引擎音效、重置飛機並累加失誤次數，2 秒後平滑重生回起點。

### 4. 攝影機追隨與 HUD 系統 (`SmoothFollowCamera.cs` & `GameManager.cs`)
- **平滑追隨攝影機**：以第三人稱視角維持於機身後上方，利用 `Vector3.Lerp` 與 `Quaternion.Slerp` 提供不暈眩且富有速度感的跟隨畫面。
- **即時 HUD 儀表板**：顯示即時飛行時速、通過光圈進度（如 `5 / 5`）、撞擊失誤次數與飛行計時；通關時跳出慶祝結算框。

### 5. 手機裝置支援與輸出測試 (Mobile Export)
- **跨平台輸入相容**：
  - 整合螢幕觸控按鈕（虛擬抬頭/俯衝/左轉/右轉十字鈕、右側加速與減速按鍵），在手機螢幕上單指點擊即可流暢操作。
  - 支援手機陀螺儀 / 重力感應 (`Input.acceleration`)，傾斜手機即可直覺控制飛行方向。
- **導出手機平台 (Build Settings)**：
  - 在 Unity 頂部選單開啟 `File -> Build Settings`。
  - 將 Platform 切換為 **Android** 或 **iOS**，點擊 `Switch Platform`。
  - 於 `Player Settings -> Resolution and Presentation` 設定預設橫向顯示 (`Landscape Left / Right`)。
  - 點擊 `Build` 即可導出 Android `.apk` 安裝檔或 iOS Xcode 專案，於實機測試運作順暢無虞。
