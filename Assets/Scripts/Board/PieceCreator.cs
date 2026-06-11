using System;
using UnityEngine;

public class PiecesCreator : MonoBehaviour
{
    [SerializeField] private ChessSkin activeSkin;

    /// <summary>
    /// Tạo quân cờ từ prefab tương ứng trong skin hiện tại, dựa trên loại và đội.
    /// </summary>
    public GameObject CreatePiece(Type type, TeamColor team)
    {
        if (activeSkin == null)
        {
            Debug.LogError("[PiecesCreator] activeSkin chưa được gán! Hãy kéo file ChessSkin Asset vào ô 'Active Skin' trên component PiecesCreator trong Inspector.");
            return null;
        }

        GameObject prefab = activeSkin.GetPrefab(type, team);
        if (prefab != null)
            return Instantiate(prefab);

        Debug.LogError($"[PiecesCreator] Prefab '{type.Name}' của đội '{team}' chưa được gán trong skin '{activeSkin.skinName}'. Hãy kiểm tra Inspector của file Skin Asset.");
        return null;
    }

    /// <summary>
    /// Trả về Material ghi đè của đội. Trả về null nếu skin không khai báo material (model đã có màu sẵn).
    /// </summary>
    public Material GetTeamMaterial(TeamColor team)
    {
        return team == TeamColor.White ? activeSkin.whiteMaterial : activeSkin.blackMaterial;
    }

    /// <summary>
    /// Đổi skin đang sử dụng. Gọi hàm này trước StartNewGame() để áp dụng skin mới.
    /// </summary>
    public void SetActiveSkin(ChessSkin newSkin)
    {
        activeSkin = newSkin;
    }
}