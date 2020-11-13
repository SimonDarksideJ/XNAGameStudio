#region File Description
//-----------------------------------------------------------------------------
// SequenceGroupData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SceneDataLibrary
{
    /// <summary>
    /// This class manages the list of pattern groups displayed in a sequence.
    /// In Layout, sequence groups correspond to these pattern groups.
    /// The Update function sets the time forward and calculates coordinates 
    /// or color values,  and the calculation results are temporarily stored 
    /// in pattern objects.
    /// These results can be used to display other display items by 
    /// synchronizing them with sequence animations.
    /// However, attention is required when creating scenes by Layout, because 
    /// it is not assumed that the same pattern object data is referred to
    /// in the same game scene.
    ///
    /// シーケンスで表示するパターングループのリストを保持します。
    /// Layoutではシーケンスグループに相当します。
    /// Update関数で、時刻を進め、座標や色情報の再計算を行います。
    /// 計算結果はパターンオブジェクトデータに一時保存されますので、
    /// その結果を用いて、他の表示物をシーケンスアニメーションに
    /// 同期させて表示させることも可能です。
    /// ただし、同じゲームシーン内で同じパターンオブジェクトデータが
    /// 参照される状況は想定していないため、
    /// Layoutでシーン作成する際は注意が必要です。
    /// </summary>
    public class SequenceGroupData
    {
        #region Public Types
        //Animation interpolation type
        //
        //アニメーション補間タイプ
        public enum Interpolation
        {
            None = 0,//No interpolation //補間無し
            Linear,//Linear interpolation //線形補間
            Spline,//Spline interpolation //スプライン補間
        }

        public enum PlayStatus
        {
            Playing = 0,//Playing //再生中
            Stop,//Stopped //停止中
            LoopEnd,//In loop //ループ中
        }
        #endregion

        #region Fields
        private int startFrame = 0;//Start frame for animation movement 
        private int loopNumber = 0;//Maximum loop count

        private TimeSpan timeFrame = new TimeSpan(0, 0, 0);//Current frame
        private PlayStatus playStatus = PlayStatus.Stop;//Play status 
        private long framePerSecond = 60;//The number of frames per second
        private int loopCount = 0;//Current loop count 
        private int drawObjectId = -1;//Base object (Object to be drawn)

        private Interpolation interpolationType = Interpolation.None;
        private int splineParamT = 0;//Spline curve parameter T
        private int splineParamC = 0;//Spline curve parameter C
        private int splineParamB = 0;//Spline curve parameter B

        private float[] tcbParam = new float[4];//Spline calculation parameter 

        //List of sequence objects 
        private List<SequenceObjectData> objectList = new List<SequenceObjectData>();
        #endregion

        #region Property
        /// <summary>
        /// Obtains and sets the start frame for animation movement.
        ///
        /// アニメーション動作の開始フレームを設定取得します。
        /// </summary>
        public int StartFrame
        {
            get
            {
                return startFrame;
            }
            set
            {
                startFrame = value;
            }
        }

        /// <summary>
        /// Obtains and sets the maximum loop count.
        ///
        /// ループ数を設定取得します。
        /// </summary>
        public int LoopNumber
        {
            get
            {
                return loopNumber;
            }
            set
            {
                loopNumber = value;
            }
        }

        /// <summary>
        /// Obtains and sets the animation interpolation type.
        /// </summary>
        public Interpolation InterpolationType
        {
            get
            {
                return interpolationType;
            }
            set
            {
                interpolationType = value;
            }
        }

        /// <summary>
        /// Obtains and sets the spline curve parameter T.
        ///
        /// スプライン補間用のパラメータを設定取得します。
        /// </summary>
        public int SplineParamT
        {
            get
            {
                return splineParamT;
            }
            set
            {
                splineParamT = value;
            }
        }

        /// <summary>
        /// Obtains and sets the spline curve parameter C.
        ///
        /// スプライン補間用のパラメータを設定取得します。
        /// </summary>
        public int SplineParamC
        {
            get
            {
                return splineParamC;
            }
            set
            {
                splineParamC = value;
            }
        }

        /// <summary>
        /// Obtains and sets the spline curve parameter B.
        ///
        /// スプライン補間用のパラメータを設定取得します。
        /// </summary>
        public int SplineParamB
        {
            get
            {
                return splineParamB;
            }
            set
            {
                splineParamB = value;
            }
        }

        /// <summary>
        /// Obtains the list of sequence objects.
        ///
        /// スプライン補間用のパラメータを設定取得します。
        /// </summary>
        public List<SequenceObjectData> SequenceObjectList
        {
            get { return objectList; }
        }

        /// <summary>
        /// If the sequence is stopped, returns true.
        ///
        /// シーケンスが停止中ならtrue
        /// </summary>
        public bool IsStop
        {
            get
            {
                return (PlayStatus.Stop == playStatus);
            }
        }

        /// <summary>
        /// If the sequence is in a loop, returns true.
        ///
        /// シーケンスがループ中ならtrue
        /// </summary>
        public bool IsLoopEnd
        {
            get
            {
                return (PlayStatus.LoopEnd == playStatus);
            }
        }
        #endregion

        
        /// <summary>
        /// Performs initialization.
        /// Converts TCB curve information to runtime format.
        ///
        /// 初期化します。
        /// TCB曲線の情報を実行時の形式に変換します。
        /// </summary>
        public void Init()
        {
            //Calculates the parameters for spline interpolation.
            //
            //スプライン補間用のパラメータの計算
            tcbParam[0] = (1.0f - SplineParamT) * (1.0f + SplineParamC) * 
                (1.0f + SplineParamB);
            tcbParam[1] = (1.0f - SplineParamT) * (1.0f - SplineParamC) * 
                (1.0f - SplineParamB);
            tcbParam[2] = (1.0f - SplineParamT) * (1.0f - SplineParamC) * 
                (1.0f + SplineParamB);
            tcbParam[3] = (1.0f - SplineParamT) * (1.0f + SplineParamC) * 
                (1.0f - SplineParamB);
        }


        /// <summary>
        /// Sets the animation time forward.
        /// Updates the current time by adding the difference between
        /// the current time and the forwarded time.
        /// If the updated time exceeds the play time of the entire animation, 
        /// consider whether to perform loop processes and determine the frames 
        /// corrected if necessary.
        ///
        /// アニメーションの時間を進めます。
        /// 現在の時刻にすすめる時間を加算して、現在の時刻を更新します。
        /// アニメーション全体の長さより現在の時刻が上回った場合は
        /// ループの有無などを検討し、必要であれば補正したフレームを割り出します。
        /// </summary>
        /// <param name="fPlayFrames">
        /// Current time
        /// 
        /// 現在の時間
        /// </param>
        /// <param name="ElapsedGameTime">
        /// Time to be forwarded
        /// 
        /// 進める時間
        /// </param>
        /// <param name="bReverse">
        /// Specifies true in case of reverse play
        /// 
        /// 逆再生の場合true
        /// </param>
        /// <returns>
        /// Returns the time specified in the sequence object to be displayed
        /// 
        /// 表示するシーケンスオブジェクトに設定された時間を返します
        /// </returns>
        public float Update(float playFrames, TimeSpan elapsedGameTime, bool reverse)
        {
            //Updates the time.
            //
            //時間の更新
            if (reverse)
                timeFrame += elapsedGameTime;
            else
                timeFrame += elapsedGameTime;

            if (TimeSpan.Zero > timeFrame)
                timeFrame = TimeSpan.Zero;

            //Clears the play status.
            //
            //再生状態のクリア
            playStatus = PlayStatus.Playing;

        BEGIN_UPDATE:

            //Calculates the length of the entire sequence.
            //
            //シーケンス全体の長さを計算します。
            int total = StartFrame;
            int length = StartFrame;
            foreach (SequenceObjectData sequenceObject in objectList)
            {
                length += sequenceObject.Frame;
            }

            //Checks whether the process proceeds to the end of the sequence.
            //
            //シーケンスの末端まで進んだかどうか確認します。
            if (timeFrame.Ticks * framePerSecond / TimeSpan.TicksPerSecond >= length)
            {
                //When the process proceeds to the end of the sequence.
                //
                //末端まで進んでいる場合。
                playStatus = PlayStatus.LoopEnd;

                //If the maximum loop count is less than 0, 
                // there is no limitation on the loop count.
                //
                //ループ規定数が負の場合、無限にループします。
                if (0 > LoopNumber)
                {
                    //Sets the status of after-loop.
                    //
                    //ループした後の状態について設定します。
                    if (1 < objectList.Count)
                    {
                        timeFrame = new TimeSpan(
                            (
                            (timeFrame.Ticks * framePerSecond / TimeSpan.TicksPerSecond)
                            % length
                            )
                            * TimeSpan.TicksPerSecond / framePerSecond);
                        
                        //Performs recalculation based on the corrected frame.
                        //
                        //補正されたフレームを元に再計算する
                        goto BEGIN_UPDATE;
                    }
                    else
                    {
                        drawObjectId = 0;
                        playStatus = PlayStatus.Stop;
                        playFrames = 0.0f;
                    }
                }
                else
                {
                    //Updates the current loop count. //ループした回数を更新します。
                    loopCount += (int)(
                        (timeFrame.Ticks * framePerSecond / TimeSpan.TicksPerSecond)
                        / length);

                    //Updates the time.
                    //
                    //時間の更新
                    timeFrame -= new TimeSpan(
                        (
                        (timeFrame.Ticks * framePerSecond / TimeSpan.TicksPerSecond)
                        % length) 
                        * TimeSpan.TicksPerSecond / framePerSecond);

                    if (loopCount < LoopNumber)
                    {
                        //Performs recalculation based on the corrected frame.
                        //
                        //補正されたフレームを元に再計算する
                        goto BEGIN_UPDATE;
                    }

                    playStatus = PlayStatus.Stop;
                }
            }
            else if (reverse && TimeSpan.Zero == timeFrame)
            {
                //In reverse play, stops playing when the frame becomes 0.
                //
                //逆再生で、フレームが０になった場合停止します。
                drawObjectId = 0;
                playStatus = PlayStatus.Stop;
                playFrames = 0.0f;
            }
            else if (timeFrame.Ticks * framePerSecond / TimeSpan.TicksPerSecond 
                >= StartFrame)
            {
                //When the process does not proceed to the end of animation, and
                //the frame is not yet 0 in reverse play.
                //(In other words, when in normal interpolation display operation.)
                //Other than these conditions,
                //
                //アニメーション末端まで進んでいない場合かつ
                //逆再生でフレーム０に至っていない場合。
                //つまり、通常の補間表示の場合。
                //これ以外の場合でも、
                drawObjectId = -1;

                int i = 0;

                //searches sequence objects to be displayed.
                //
                //表示対象にするシーケンスオブジェクトを検索します。
                foreach (SequenceObjectData data in objectList)
                {
                    total += data.Frame;

                    if (timeFrame.Ticks * framePerSecond / TimeSpan.TicksPerSecond 
                        < total)
                    {
                        //The object to be displayed is found.
                        //
                        //表示するオブジェクトを発見
                        drawObjectId = i;
                        break;
                    }

                    i++;
                }

                //When the object to be displayed is found.
                //
                //表示するオブジェクトが見つかった場合
                if (0 <= drawObjectId)
                {
                    //Calculates the time in the sequence object to be displayed.
                    //
                    //表示するシーケンスオブジェクト内での時間を計算します。
                    if (0 == objectList[drawObjectId].Frame)
                    {
                        playFrames = 0.0f;
                    }
                    else
                    {
                        playFrames = (
                            (float)timeFrame.Ticks * (float)framePerSecond /
                            (float)TimeSpan.TicksPerSecond - 
                            (float)(total - objectList[drawObjectId].Frame)) 
                            / (float)objectList[drawObjectId].Frame;
                    }
                }
            }
            else
            {
                //Displays nothing.
                //
                //何も表示しない
                drawObjectId = -1;
            }

            // Once the frame has been set up, performs interpolation 
            // for conversion information.
            //
            //フレームが確定したので、変換情報の補間処理を行います。
            updateSeq(playFrames);

            return playFrames;
        }


        /// <summary>
        /// Performs interpolation based on the provided display frame.
        /// Begins by determining the sequence objects to be displayed. 
        /// Then, if the interpolation type is Linear or Spline, performs
        /// conversion interpolation by using the information of neighboring 
        /// sequence objects as needed.
        ///
        /// 与えられた表示フレームを元に補間処理を行います。
        /// まず、表示すべきシーケンスオブジェクトを求め、
        /// 補間タイプが線形、スプラインの場合は、
        /// 必要に応じて近隣のシーケンスオブジェクトの情報を用いて
        /// 変換の補間を行います。
        /// </summary>
        /// <param name="fPlayFrames">
        /// Time in the current sequence object to be displayed
        /// 
        /// 現在表示すべきシーケンスオブジェクト内での時間
        /// </param>
        private void updateSeq(float playFrame)
        {
            //The following objects are needed for interpolation (at a maximum):
            //- Base object ...baseObject
            //- Previous object (Object before the Base object) ...prevObject
            //- Target object (Object after the Base object) ...targetObject
            //- Next object (Object after the Target object) ...nextObject
            //
            //補間に必要なのは、最大で
            //・着目オブジェクト...baseObject
            //・着目オブジェクトの前のオブジェクト...prevObject
            //・着目オブジェクトの次のオブジェクト...targetObject
            //・着目オブジェクトの次の次のオブジェクト...nextObject
            //になります。
            SequenceObjectData prevObject, baseObject, targetObject, nextObject;

            //If there is no object to be displayed, returns.
            //
            //表示するオブジェクトが無いなら戻ります。
            if (0 > drawObjectId)
                return;

            //Sets the objects used for interpolation.
            //
            //補間のために使用するするオブジェクトを設定します。

            //Clears the objects used for interpolation.
            //
            //補間用のオブジェクトのクリア
            nextObject = targetObject = prevObject = baseObject
                                            = objectList[drawObjectId];
            //Obtains the Previous object.
            //
            //直前のオブジェクトの取得
            if (0 < drawObjectId)
            {
                prevObject = objectList[drawObjectId - 1];
            }
            else if (0 != loopCount)
            {
                //Otherwise, uses the last object when performing the loop process.
                //
                //そうでない場合、ループするなら最後のオブジェクトを用いる
                prevObject = objectList[objectList.Count - 1];
            }

            //Obtains the Target object (and sets the Next object in advance).
            //
            //次のオブジェクトの取得("次の次"もあらかじめ設定)
            if (drawObjectId < objectList.Count - 1)
            {
                //When the Base object is not the last object.
                //
                //着目オブジェクトがが最後のオブジェクトではない。
                nextObject = targetObject = objectList[drawObjectId + 1];
            }
            else
            {
                //Sets the first object when performing the loop process.
                //
                //ループするなら、最初のオブジェクトに設定
                if (loopCount < LoopNumber)
                {
                    nextObject = targetObject = objectList[0];
                }
            }

            //Obtains the Next object.
            //
            //次の次のオブジェクトの取得
            if (drawObjectId < objectList.Count - 2)
            {
                //When the current index + 2 is valid.
                //
                //２つ先のIndexが有効
                nextObject = objectList[drawObjectId + 2];
            }
            else
            {
                //Loop process
                //
                //ループする
                if (loopCount < LoopNumber)
                {
                    //When the Next object is the first object.
                    //
                    //次の次は最初のオブジェクト。
                    if (drawObjectId == objectList.Count - 2)
                    {
                        nextObject = objectList[0];
                    }
                    else
                    {
                        //When there are 2 or more objects in total (Index 1 is valid).
                        //
                        //全体のオブジェクト数が2つ以上（Index１が有効）
                        if (1 < objectList.Count)
                            nextObject = objectList[1];
                        else
                            nextObject = objectList[0];
                    }
                }
            }

            //For each pattern object in the sequence object to be displayed, 
            //records the interpolated conversion information.
            //
            //表示するシーケンスオブジェクト内のパターンオブジェクトそれぞれについて
            //補間した変換情報を記録していきます。
            for (int i = 0; i < baseObject.PatternObjectList.Count; i++)
            {
                PatternObjectData prevPattern, basePattern;
                PatternObjectData targetPattern, nextPattern;

                //Sets the interpolation target pattern.
                //
                //補間対象のパターンを設定します。
                prevPattern = basePattern = targetPattern = 
                    nextPattern = baseObject.PatternObjectList[i];

                if (targetObject.PatternObjectList.Count > i)
                    targetPattern = targetObject.PatternObjectList[i];

                if (prevObject.PatternObjectList.Count > i)
                    prevPattern = prevObject.PatternObjectList[i];

                if (nextObject.PatternObjectList.Count > i)
                    nextPattern = nextObject.PatternObjectList[i];

                //Calculates conversion values after interpolation.
                //
                //補間後の変換情報を計算します。
                DrawData data = new DrawData();
                switch (InterpolationType)
                {
                    case Interpolation.None:
                        data = basePattern.Data;
                        break;
                    case Interpolation.Linear:
                        PutInfoLinearInterporlation(playFrame, data, 
                            basePattern.Data, targetPattern.Data);
                        break;
                    case Interpolation.Spline:
                        PutInfoTCBSplineInterporlation(playFrame, data, 
                            prevPattern.Data, basePattern.Data, 
                            targetPattern.Data, nextPattern.Data);
                        break;
                }

                //Records the data to the pattern object.
                //The data recorded here can be used as position information for 
                //animated patterns.
                //
                //パターンオブジェクトに記録します。
                //ここで記録された情報は、アニメーションされたパターンの
                //配置情報として利用することが可能です。
                baseObject.PatternObjectList[i].InterpolationDrawData = data;

            }
        }

        /// <summary>
        /// Draws the objects by using the conversion information modified 
        /// by the Update function.  The conversion information can be also applied
        /// to the entire sequence (as offset).
        ///
        /// Update関数で更新された変換情報を利用しながら描画します。
        /// シーケンス全体に変換を（オフセットとして）適用することも出来ます。
        /// </summary>
        /// <param name="sb">
        /// SpriteBatch
        /// 
        /// スプラインバッチ
        /// </param>
        /// <param name="fPlayFrames">
        /// Frame to be displayed
        /// 
        /// 表示するフレーム
        /// </param>
        /// <param name="infoPut">
        /// Conversion information that affects the entire drawing target
        /// 
        /// 描画対象全体に影響する変換情報
        /// </param>
        public void Draw(SpriteBatch sb, DrawData baseDrawData)
        {
            if (0 > drawObjectId)
            {
                return;
            }

            //Obtains the current Base object.
            //
            //現在着目しているシーケンスオブジェクトを取得
            SequenceObjectData baseObj = objectList[drawObjectId];

            //Referred to by the sequence object.
            //
            //シーケンスオブジェクトが参照する
            for (int i = 0; i < baseObj.PatternObjectList.Count; i++)
            {
                DrawData sqInfo =
                    baseObj.PatternObjectList[i].InterpolationDrawData;
                baseObj.PatternObjectList[i].Draw(sb, sqInfo, baseDrawData);
            }
        }

        /// <summary>
        /// Performs simple linear interpolation.
        ///
        /// 単純な線形補間をします。
        /// </summary>
        /// <param name="rate">
        /// If the interpolation rate is 1.0, the value will be that of "target".
        /// 
        /// 補間割合1.0ならtargetの値になります。
        /// </param>
        /// <param name="fTarget">
        /// Target value
        /// 
        /// 目的値
        /// </param>
        /// <param name="fBase">
        /// Initial value
        /// 
        /// 初期値
        /// </param>
        /// <returns>
        /// Interpolation results
        /// 
        /// 補間結果
        /// </returns>
        private static float LinearInterporlation(float rate, float targetValue, 
            float baseValue)
        {
            return (targetValue * rate + baseValue * (1f - rate));
        }

        /// <summary>
        /// Performs linear interpolation for the conversion information 
        /// (position, rotation, scale, center point, and color).
        ///
        /// 変換情報(位置・回転・スケール・中心点・色)を線形補間します。
        /// </summary>
        /// <param name="rate">
        /// Interpolation rate
        /// 
        /// 補間割合
        /// </param>
        /// <param name="resultInfo">
        /// Calculation results will be stored.
        /// 
        /// 計算結果が入ります。
        /// </param>
        /// <param name="BaseInfo">
        /// Initial value
        /// 
        /// 初期値
        /// </param>
        /// <param name="targetInfo">
        /// Target value
        /// 
        /// 目的値
        /// </param>
        private static void PutInfoLinearInterporlation(float rate, DrawData resultData,
                                DrawData baseData, DrawData targetData)
        {
            resultData.Position = new Point(
                (int)LinearInterporlation(rate, targetData.Position.X, 
                                            baseData.Position.X),
                (int)LinearInterporlation(rate, targetData.Position.Y, 
                                            baseData.Position.Y)
            );
            resultData.Color = new Color(
                (byte)LinearInterporlation(rate, targetData.Color.R, 
                                                        baseData.Color.R),
                (byte)LinearInterporlation(rate, targetData.Color.G, 
                                                        baseData.Color.G),
                (byte)LinearInterporlation(rate, targetData.Color.B, 
                                                        baseData.Color.B),
                (byte)LinearInterporlation(rate, targetData.Color.A, 
                                                        baseData.Color.A)
            );
            resultData.Scale = new Vector2(
                LinearInterporlation(rate, targetData.Scale.X, baseData.Scale.X),
                LinearInterporlation(rate, targetData.Scale.Y, baseData.Scale.Y));
            resultData.Center = new Point(
                (int)LinearInterporlation(rate, targetData.Center.X, 
                baseData.Center.X), 
                (int)LinearInterporlation(rate, targetData.Center.Y, 
                baseData.Center.Y));
            resultData.RotateZ = LinearInterporlation(rate, targetData.RotateZ, 
                baseData.RotateZ);
        }

        /// <summary>
        /// Performs spline interpolation by using TCB curve.
        ///
        /// TCB曲線を用いたスプライン補間を行います。
        /// </summary>
        /// <param name="rate">
        /// Interpolation rate
        /// 
        /// 補間割合
        /// </param>
        /// <param name="prevValue">
        /// Previous value of the initial value
        /// 
        /// 初期値の前の値
        /// </param>
        /// <param name="baseValue">
        /// Initial value
        /// 
        /// 初期値
        /// </param>
        /// <param name="targetValue">
        /// Target value
        /// 
        /// 目標値
        /// </param>
        /// <param name="nextValue">
        /// Next value of the target value
        /// 
        /// 目標値の次の値
        /// </param>
        /// <returns>
        /// Calculation results
        /// 
        /// 計算結果
        /// </returns>
        private float CalcSpline(float rate, float prevValue, 
            float baseValue, float targetValue, float nextValue)
        {
            float fRate = rate * rate,
                  fRate2 = fRate * rate;
            float fQ0 = tcbParam[0] * (baseValue - prevValue) + tcbParam[1] *
                        (targetValue - baseValue);
            float fQ1 = tcbParam[2] * (targetValue - baseValue) + tcbParam[3] * 
                        (nextValue - targetValue);

            return ((2.0f * baseValue - 2.0f * targetValue + fQ0 + fQ1) * fRate2 + 
                (-3.0f * baseValue + 3.0f * targetValue - 2.0f * fQ0 - fQ1) * 
                fRate + fQ0 * rate + baseValue);
        }

        /// <summary>
        /// Performs interpolation for the conversion information (position, rotation,
        /// scale, center position, color) by using TCB curve.
        ///
        /// TCB曲線を用いた変換情報(位置・回転・スケール・中心点・色)の補間を行います。
        /// </summary>
        /// <param name="rate">
        /// Interpolation rate
        /// 
        /// 補間割合
        /// </param>
        /// <param name="resultInfo">
        /// Calculation results
        /// 
        /// 計算結果
        /// </param>
        /// <param name="prevInfo">
        /// Previous value of the initial value
        /// 
        /// 初期値の前の値
        /// </param>
        /// <param name="baseInfo">
        /// Initial value
        /// 
        /// 初期値
        /// </param>
        /// <param name="targetInfo">
        /// Target value
        /// 
        /// 目標値
        /// </param>
        /// <param name="nextInfo">
        /// Next value of the target value
        /// 
        /// 目標値の次の値
        /// </param>
        private void PutInfoTCBSplineInterporlation(float rate,
            DrawData resultData, DrawData prevData, 
            DrawData baseData, DrawData targetData, DrawData nextData)
        {
            resultData.Position = new Point(
                (int)CalcSpline(rate, prevData.Position.X, baseData.Position.X, 
                                targetData.Position.X, nextData.Position.X),
                (int)CalcSpline(rate, prevData.Position.Y, baseData.Position.Y, 
                                targetData.Position.Y, nextData.Position.Y));
            resultData.Color = new Color(
                (byte)CalcSpline(rate, prevData.Color.R, baseData.Color.R, 
                                targetData.Color.R, nextData.Color.R),
                (byte)CalcSpline(rate, prevData.Color.G, baseData.Color.G, 
                                targetData.Color.G, nextData.Color.G),
                (byte)CalcSpline(rate, prevData.Color.B, baseData.Color.B, 
                                targetData.Color.B, nextData.Color.B),
                (byte)CalcSpline(rate, prevData.Color.A, baseData.Color.A, 
                                targetData.Color.A, nextData.Color.A));
            resultData.Scale = new Vector2(
                CalcSpline(rate, prevData.Scale.X, baseData.Scale.X, 
                                targetData.Scale.X, nextData.Scale.X),
                CalcSpline(rate, prevData.Scale.Y, baseData.Scale.Y, 
                                targetData.Scale.Y, nextData.Scale.Y));
            resultData.Center = new Point(
                (int)CalcSpline(rate, prevData.Center.X, baseData.Center.X, 
                                targetData.Center.X, nextData.Center.X),
                (int)CalcSpline(rate, prevData.Center.Y, baseData.Center.Y, 
                                targetData.Center.Y, nextData.Center.Y));
            resultData.RotateZ = CalcSpline(rate, prevData.RotateZ, 
                                baseData.RotateZ, targetData.RotateZ, nextData.RotateZ);
        }

        /// <summary>
        /// Resets the frame and plays it from the beginning.
        ///
        /// フレームをリセットして最初から再生します。
        /// </summary>
        public void Replay()
        {
            timeFrame = new TimeSpan();
        }

        /// <summary>
        /// Based on the conversion information obtained and interpolated by the 
        /// Update function, the sequence group displays pattern objects in the 
        /// pattern group that is referred to by the current Base sequence object.
        /// This function obtains this current Base sequence object.
        ///
        /// シーケンスグループは、着目しているシーケンスオブジェクトが
        /// 参照するパターングループ内のパターンオブジェクトを
        /// Update関数で求めた補間された変換情報に基づいて表示します。
        /// この、現在着目しているシーケンスオブジェクトを取得する関数です。
        /// </summary>
        /// <returns>
        /// Current sequence object to be displayed
        /// 
        /// 現在表示することになっているシーケンスオブジェクト
        /// </returns>
        [ContentSerializerIgnore]
        public SequenceObjectData CurrentObjectList
        {
            get
            {
                return (drawObjectId >= 0) ? objectList[drawObjectId] : objectList[0];
            }
        }

    }
}
