﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeboCam
{
    public class statistics
    {

        public static int statisticsCountLimit = 10000;

        private class movement
        {

            public int cameraId;
            public int motionLevel;
            public Int64 milliSecondsSinceStart;
            public string profile;
            public DateTime dateTime;
            public bool statReturnedPing;
            public bool statReturnedPublish;
            public bool statReturnedOnline;
            public bool statReturnedAlert;

        }

        private class LastMovement
        {

            public int cameraId;
            public int motionLevel;
            public Int64 milliSecondsSinceStart;
            public string profile;

        }

        public class movementResults
        {

            public int avgMvStart;
            public int avgMvLast;
            public int mvNow;

        }


        private static List<movement> statList = new List<movement>();
        private static List<LastMovement> lastMovementList = new List<LastMovement>();

        public static void add(int p_cameraId, int p_motionLevel, Int64 p_milliSsecondsSinceStart, string p_profile)
        {

            insertIfValueChanged(p_cameraId, p_motionLevel, p_milliSsecondsSinceStart, p_profile);

        }

        public static int lowestValTime(int p_cameraId, int p_milliseconds, string p_profile)
        {

            int lowestVal = 100;
            long startMilli = new long();
            long lastMilli = new long();

            //start from teh most recenbt statistic
            for (int i = statList.Count - 1; i >= 0; i--)
            {

                //we have a match on camera and profile
                if (statList[i].cameraId == p_cameraId && statList[i].profile == p_profile)
                {


                    //first time through so we set the last time to the 
                    //most recent time recorded
                    if (startMilli == 0)
                    {

                        startMilli = statList[i].milliSecondsSinceStart;

                    }

                    //the time between the start time and current stat is less than 
                    //or equal to the time frame 
                    if (startMilli - statList[i].milliSecondsSinceStart <= p_milliseconds)
                    {


                        //the level is lower than the lowest level found so far
                        if (statList[i].motionLevel < lowestVal)
                        {

                            lowestVal = statList[i].motionLevel;
                            lastMilli = statList[i].milliSecondsSinceStart;

                            System.Diagnostics.Debug.Print(lowestVal.ToString());

                            if (lowestVal == 0)
                            {

                                System.Diagnostics.Debug.Print("zero value");

                            }

                        }

                    }
                    else
                    {

                        //find if there is a lower value just before the current value
                        // as we need to account for gaps were values remain the same
                        if (i > 0)
                        {

                            for (int a = i - 1; a >= 0; a--)
                            {

                                if (statList[a].cameraId == p_cameraId
                                    && statList[a].profile == p_profile
                                    && statList[a].motionLevel < lowestVal)
                                {

                                    lowestVal = statList[a].motionLevel;
                                    break;

                                }

                            }

                        }

                        break;

                    }


                }


            }

            try
            {

                return lowestVal;

            }
            catch (Exception)
            {


                return 0;

            }


        }



        public static void clear()
        {

            statList.Clear();
        }


        private static void insertIfValueChanged(int p_cameraId, int p_motionLevel, Int64 p_milliSsecondsSinceStart, string p_profile)
        {

            int statsLimit = statisticsCountLimit;
            bool found = false;

            //keep the list os statistics to a reasonable size
            if (statList.Count > statsLimit)
            {

                statList.RemoveRange(0, 1);

            }

            foreach (LastMovement item in lastMovementList)
            {

                if (item.cameraId == p_cameraId && item.profile == p_profile)
                {

                    found = true;
                    if (item.motionLevel != p_motionLevel)
                    {

                        item.milliSecondsSinceStart = p_milliSsecondsSinceStart;
                        item.motionLevel = p_motionLevel;

                        movement mv = new movement();
                        mv.cameraId = p_cameraId;
                        mv.motionLevel = p_motionLevel;
                        mv.milliSecondsSinceStart = p_milliSsecondsSinceStart;
                        mv.dateTime = DateTime.Now;
                        mv.profile = p_profile;

                        statList.Add(mv);

                    }

                    break;

                }

            }


            if (!found)
            {


                LastMovement mov = new LastMovement();
                mov.cameraId = p_cameraId;
                mov.milliSecondsSinceStart = p_milliSsecondsSinceStart;
                mov.motionLevel = p_motionLevel;
                mov.profile = p_profile;
                lastMovementList.Add(mov);


            }


        }


        public static movementResults statsForCam(int icameraId, string iprofile, string imageType)
        {

            int firstCount = new int();
            int firstSum = new int();
            int lastCount = new int();
            int lastSum = new int();
            int currMv = new int();

            firstCount = 0;
            firstSum = 0;
            lastCount = 0;
            lastSum = 0;
            currMv = 0;


            foreach (movement mv in statList)
            {

                if (mv.cameraId == icameraId && mv.profile == iprofile)
                {

                    bool statsReturned = new bool();

                    switch (imageType)
                    {
                        case "Publish":
                            statsReturned = mv.statReturnedPublish;
                            break;
                        case "Online":
                            statsReturned = mv.statReturnedOnline;
                            break;
                        case "Ping":
                            statsReturned = mv.statReturnedPing;
                            break;
                        case "Alert":
                            statsReturned = mv.statReturnedAlert;
                            break;
                        default:
                            statsReturned = mv.statReturnedPublish;
                            break;
                    }

                    if (statsReturned)
                    {

                        firstCount++;
                        firstSum += mv.motionLevel;

                    }
                    else
                    {

                        firstCount++;
                        firstSum += mv.motionLevel;
                        lastCount++;
                        lastSum += mv.motionLevel;

                    }

                    currMv = mv.motionLevel;

                    switch (imageType)
                    {
                        case "Publish":
                            mv.statReturnedPublish = true;
                            break;
                        case "Online":
                            mv.statReturnedOnline = true;
                            break;
                        case "Ping":
                            mv.statReturnedPing = true;
                            break;
                        case "Alert":
                            mv.statReturnedAlert = true;
                            break;
                        default:
                            mv.statReturnedPublish = true;
                            break;
                    }




                }

            }

            movementResults mvR = new movementResults();
            mvR.avgMvLast = (int)Math.Floor((double)lastSum / (double)lastCount);
            mvR.avgMvStart = (int)Math.Floor((double)firstSum / (double)firstCount);

            mvR.avgMvLast = mvR.avgMvLast > 0 ? mvR.avgMvLast : 0;
            mvR.avgMvStart = mvR.avgMvStart > 0 ? mvR.avgMvStart : 0;

            mvR.mvNow = currMv;

            return mvR;

        }




    }
}