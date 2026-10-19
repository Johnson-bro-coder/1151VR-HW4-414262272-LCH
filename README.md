# 1151VR-HW4-414262272-LCH

## 一、專案截圖

1. 圖一

    <img width="670" height="418" alt="image" src="https://github.com/user-attachments/assets/096000a1-5599-49fc-a83d-38868cef62e1" />

    
2. 圖二
<img width="717" height="435" alt="image" src="https://github.com/user-attachments/assets/84105bdd-9ca8-474f-ad24-023dc4618254" />


---

## 二、GitHub 連結

- **GitHub Repository**: https://github.com/Johnson-bro-coder/1151VR-HW4-414262272-LCH

---

## 三、YouTube 連結

- **YouTube 演示影片**: https://youtu.be/tnMkfkqxx-c

---

## 四、說明製作流程和相關操作

### 1. Unity 3D 專案建置與資源匯入

- **3D 模型資源**：
    - `Airplane.obj`：3D 飛行器模型。
    - `RingObstacle.obj`：環狀穿透光圈障礙物。
    - `PillarObstacle.obj`：垂直石柱障礙物。
- **音效資源 (Audio Clips)**：
    - `Airplane_Fly_Loop.wav`：飛行巡航持續引擎聲。
    - `Obstacle_Crash.wav`：撞擊外框或石柱時觸發的墜毀爆炸破裂音效。
    - `Pass_Ring_Success.wav`：精準穿過光圈內部時觸發的悅耳通關提示和弦音。

### 2. 3D 飛行控制與空氣動力邏輯

- **前進推力與速度調節**：
    - 機身維持恆定巡航基礎速度，提供 Shift / Space 油門加速 與 Ctrl 減速 機制。
    - **俯仰** ：W/S 或 方向鍵上下，調整機頭仰角與俯衝。
    - **偏航** ：A/D 或 方向鍵左右，控制航向水平轉彎。
    - **滾轉** ：Q/E 鍵進行特技側滾；左右轉向時自動帶入空氣動力側傾角，大幅提升飛行真實感。
    - **水平自動回正** ：玩家釋放轉向鍵時，自動平滑校正機身至水平平衡姿態。
- **動態音效連動**：
    - 根據當前飛行速度與最大速度比例，即時調整音效，模擬油門增壓音效。

### 3. 障礙物判定與音效反饋機制

- **穿透式光圈障礙**：
    - 偵測飛行器穿入。
    - 成功判定後播放 音效，光圈顏色由金黃色即時變為翠綠色，並於 HUD 累加通過數量（`passedRings++`）。
- **實體危險障礙物** ：
    - 設置於石柱、光圈外框及地面。
    - 撞擊瞬間觸發中斷引擎音效、重置飛機並累加失誤次數，2 秒後平滑重生回起點。

### 4. HUD 系統

- **即時 HUD 儀表板**：顯示即時飛行時速、通過光圈進度（如 `5 / 5`）、撞擊失誤次數與飛行計時；通關時跳出慶祝結算框。

