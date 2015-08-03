# Onion Ring for C# #

Open [Window] > [Onion Ring Sample]

以下のQiitaでの記事を元に、Unityで使いやすくするためにC#で書きなおしたものです。
kyubunsさん、良いプログラムをありがとうございます。

> 自動で9 sliced sprite(9 patch)画像を生成してくれるモジュール作った
> http://qiita.com/kyubuns/items/cb01f926966b51a5501c

## プログラム例　
```Sample.cs
var before = "Assets/Sample/Resources/before.png";
var after = "Assets/Sample/Resources/after.png";

var border = OnionRing.Run(before, after);
Debug.Log(border.ToString());
```

* before
![image](https://raw.githubusercontent.com/uqtimes/OnionRingCSharp/master/Assets/Sample/Resources/before.png)

* after
![image](https://raw.githubusercontent.com/uqtimes/OnionRingCSharp/master/Assets/Sample/Resources/after.png)

Support for Unity 4 and Unity 5.

## 使用画像
[https://pixabay.com/ja/矢印-戻る-前-アイコンを-シンボル-ビジネス-記号-デザイン-42915/](https://pixabay.com/ja/%E7%9F%A2%E5%8D%B0-%E6%88%BB%E3%82%8B-%E5%89%8D-%E3%82%A2%E3%82%A4%E3%82%B3%E3%83%B3%E3%82%92-%E3%82%B7%E3%83%B3%E3%83%9C%E3%83%AB-%E3%83%93%E3%82%B8%E3%83%8D%E3%82%B9-%E8%A8%98%E5%8F%B7-%E3%83%87%E3%82%B6%E3%82%A4%E3%83%B3-42915/)
