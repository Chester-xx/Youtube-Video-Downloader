   M S B A M L   `   `   `  ���� K  GVideo Downloader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:ProgramProgram      Program.MainWindow   W SPresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     'localclr-namespace:Program     -     �Name$  frmMain�� � Title$ Video Downloader��$	��800��$
��1400�� � WindowStartupLocation$ CenterScreen=� � Icon$ 	/icon.jpg�� � 
ResizeMode$ CanMinimize=� � WindowStyle$
 None=� � AllowsTransparency$
 True��
������ ��Z� .���� $��1q� �� CornerRadius$ 20,20,20,20��
������ ._�� -    �Name$ grdMain�����	#FF191919$��Center=�$��Center=�$	��796��$
��1400��	 � ColumnDefinitions	 �� $��0* ��� $
��107* ��� $
��173* �.+�]� -   
 ]�Name$
 
DpTitleBar��$��False��$��1��$��	0,0,0,764q�$��2�����	#FF191919$��32��.+��� -    ��Name$ BtnClose��$��×��$��Right=�$��45��$��32�� �� 	ClickMode$ Release=���Z� ����  �� 
TargetType" ��.���  �� Property � �� �� Value �	#FF676767��  � �� �	#FF191919��  � ��$ Center=���  � ��$ Center=� �� Triggers P�  P� Property �@J�IsMouseOver P� Value$
 True��. ���  � ��
 �  ����  � �� �	#FF2D2D2D����  �� 
TargetType" ��.R��� #��z��
������ $��0q�._��� $��Center=�$��Center=�$��0, -7.5, 0.7, 0q�$��30(�$
��Thin"��� -   $ btnMinimize��$��Right=�$��45��$��32��$��-��$ Release=���Z� ���� " ��.���  � �� �	#FF676767��  � �� �	#FF191919��  � ��$ Center=���  � ��$ Center=� P�  �@J�IsMouseOver$
 True��. ���  � �� �	#FF676767��  � �� �	#FF2D2D2D���� " ��.R��� #��z��
������ $��0q�._��� $��Center=�$��Center=�$��0, -15.5, 1, 0q�$��40(�$
��Thin"�� -   $ grdTitleBar��$	��346��.+��� $��
19,6,307,6q�$��	/icon.jpg���� -    ��Name$ lblTitleBar��$��Video Downloader��$��44,0,0,0q�
�������$��Center=�$
��Left=��� -   $ btnDownload��$��Download Video��$��2�����	#FF197DCC$
��Bold"�$��24(�$��75,610,155,116q����	#FF464646 � ToolTip$ Start your download.����Z� ���� " ��.R���  ��Name$ border��#��z��#��z��$ 10�� �� Padding$ 10q�._��� $��Center=�$��Center=� �� Triggers P�  �@J�IsMouseOver$
 True��. ���  �� 
TargetName$ border�� � �� �	#FF505050P�  � ��$
 True��. ��� $ border�� � �� �	#FF303030�� -	   $ btnDirectory��$
��📁��$��2�����	#FF197DCC$��24(�$��716,610,75,116q����	#FF464646$) #Select Download folder destination.����Z� ���� " ��.R��� $ border��#��z��#��z��$ 10��$ 10q�._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ��� $ border�� � �� �	#FF505050P�  � ��$
 True��. ��� $ border�� � �� �	#FF303030�� -
    ��Name$ txtURL�� �� TextWrapping$
 Wrap=�$��2��$��75,383,75,377q����	#FF636363$��18(�
������� �� TextAlignment$ Center=� �� SelectionBrush �	#FF197DCC$��Center=�$��Center=�$ Paste Your Link Here��$��⌘ Paste Link Here����Z� �� -   $ bdrView�����	#FF197DCC$��1,1,1,1q�$��Center=�$	��Top=�$	��728��$	��498��$��1��$ 10,10,10,10��$��0,42,0,0q�._��� -   $ bdrContainerImage�����	#FF197DCC$��1q�$��48,48,48,453q�$ 10,10,10,10��._��� -    ��Name$ imgThumbNail�� � IsSharedSizeScope$
 True��$��UniformToFill=�$	��400��$	��225��  J� Clip  �� ! �� Rect$! 0,0,398,223�" �� RadiusX$" 10Q�# �� RadiusY$# 10Q��� -   $ lblDownloadInfo��$��Download Information��$��Center=�$��2��$��159,519,159,247q����	#FF197DCC$��16(�$
��Bold"��� -   $ 	lblOutput��$��1��$��Output :��$��Center=�$��	0,622,0,0q�$	��Top=�
�������$	��397��$	��141��� -   $ �Name$$ prgbarDownload��$��3��$��10��$��	0,786,0,0q����	#FF464646���	#FF464646���	#FF0078D7�� -   $ lblSpeed��$��2��$��69,584,69,181q�
�������$��Speed :����Z� �� -   $ lblDirectory��$!��Download Destination : None��$��2��$��68,552,69,219q�
��������� -   $ lblTitle��$��2��$��68,318,69,436q�$��Center=�
�������$��22(�$
��Bold"���Z� �� -   $ lblViewCount��$��2��$��69,365,69,405q�
�������$��Views :����Z� �� -   $ 	lblAuthor��$��68,394,69,371q�$��1��
�������$��31��$��Author :���� -   $ lblLikeCount��$��1��$��69,427,69,343q�
�������$��Likes :���� -   $ 	lblFormat��$��1��$��69,454,69,312q�$��30��
�������$��Format :���� -   $ lblURL��$��69,486,69,284q�$��1��
�������$��Link :���� -   $ lblVideoInfo��$��Video Information��$��Center=�$��2��$��158,55,159,711q����	#FF197DCC$��16(�$
��Bold"�	� -   % 	�Name$% rgbCombined��& 	� 	GroupName$& options��' d� 	IsChecked$
' TrueY�$��2��$��74,539,612,221q����	#FF464646$	��179��$��1q�$B <Downloads and merges video and audio into a single mp4 file.������ " 	�.R��� $ border��$ 	10,0,0,10��$ 10q�#��z��._��� $��Center=�( �� Orientation$( 
Horizontal=�.+��� $��	Standard ���� $	��✨�� P�  �@d�	IsChecked$
 TrueY�. ��� $ border�� � �� �	#FF197DCCP�  �@J�IsMouseOver$
 True��. ��� $ border�� � �� �	#FF197DCC	� -   $% rgbVideo��$& options��$��253,539,433,221q�$��2��$	��179�����	#FF464646$- 'Downloads video only. Audio is removed.������ " 	�.R��� $ border��$ 0,0,0,0��$ 10q�#��z��._��� $��Center=�$( 
Horizontal=�.+��� $��
Video Only��$��0,0,5,0q��� $
��📸�� P�  �@d�	IsChecked$
 TrueY�. ��� $ border�� � �� �	#FF197DCCP�  �@J�IsMouseOver$
 True��. ��� $ border�� � �� �	#FF197DCC	� -   $% rgbVideoAndAudio��$& options��$��432,539,254,221q�$��2��$	��179�����	#FF464646$E ?Downloads video and audio into two seperate files, mp4 and mp3.������ " 	�.R��� $ border��$ 0,0,0,0��$ 10q�#��z��._��� $��Center=�$( 
Horizontal=�.+��� $��Video + Audio��$��0,0,5,0q��� $��📸🔊�� P�  �@d�	IsChecked$
 TrueY�. ��� $ border�� � �� �	#FF197DCCP�  �@J�IsMouseOver$
 True��. ��� $ border�� � �� �	#FF197DCC	� -   $% rgbAudio��$& options��$��611,539,75,221q�$��2��$	��179�����	#FF464646$- 'Downloads audio only. Video is removed.������ " 	�.R��� $ border��$ 	0,10,10,0��$ 10q�#��z��._��� $��Center=�$( 
Horizontal=�.+��� $��
Audio Only��$��0,0,5,0q��� $
��🔉�� P�  �@d�	IsChecked$
 TrueY�. ��� $ border�� � �� �	#FF197DCCP�  �@J�IsMouseOver$
 True��. ��� $ border�� � �� �	#FF197DCC�� -   $ 	lblStatus��$��Center=�$��Center=�$��2��$��75,146,75,538q�
�������$��36(�$
��Bold"�$��Waiting for link...��