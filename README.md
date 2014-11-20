# MEMEs Image Converter

任意の画像をMEMEsボードのTFTに表示できるように，RGB=5:6:5の形式に変換するプログラムです．

[MEMEsボードのサポートページ](http://memes.sakura.ne.jp/memes/)

## 出力される画像ファイルのフォーマット

全てのデータは2bytes bidendianが1単位のフォーマットです。

出力されるファイルは以下のフォーマットになります。

1. 画像サイズ情報
  * 幅(2bytes)
  * 高さ(2bytes)
2. 画素情報
 	* 1画素あたりRGB = 5:6:5の16bitで構成
 	* Rが最上位ビット，Bが最下位ビットの順で格納されます
 	* 画像の左上から右下へ向かう順で画素情報が保存されます

## 動作環境・開発環境

Windows環境のみで動作します。

* Visual Studio 2013
* C#(.NET Framework 4.5)
* Windows Form Application

## License

* MIT License

## Copyright

Copyright (c) 2014 Hayato OKUMOTO, jo-ken.info