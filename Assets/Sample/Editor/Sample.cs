using OnionRingCSharp;
using UnityEditor;
using UnityEngine;

public class Sample : MonoBehaviour
{
    [MenuItem("Window/Onion Ring Sample")]
    static void OnionRingSample()
    {
        // OnionRingで処理する画像へのパス
        var before = "Assets/Sample/Resources/before.png";
        var after = "Assets/Sample/Resources/after.png";

        // 事前にRead/Writeにする
        var importer = AssetImporter.GetAtPath(before) as TextureImporter;
        importer.isReadable = true;

        // 設定を変更した画像情報を更新
        AssetDatabase.ImportAsset(before, ImportAssetOptions.ForceSynchronousImport);

        // OnionRingでの処理開始
        var border = OnionRing.Run(before, after);

        // 実行結果のborderを出力: [497,1,152,867]
        Debug.Log(border.ToString());

        // 出来上がった画像のアセットをリロード
        AssetDatabase.ImportAsset(after, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.Refresh();
    }
}
