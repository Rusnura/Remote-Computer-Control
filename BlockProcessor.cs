using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace RemoteComputerControl
{
    public class BlockProcessor
    {
        private static BlockProcessor instanse = null;
        private BlockingQueue<BlockingSoft> blockingQ = null;
        private Thread blockThreading;

        private BlockProcessor(BlockingQueue<BlockingSoft> blockingQ, UserAPI userAPI) 
        {
            this.blockingQ = blockingQ;
            this.blockThreading = new Thread(blockingThreadFx);
            this.blockingQ.addAll(userAPI.loadBlockedSoftFromStorageOrString(null));
            this.blockThreading.Start();
        }

        public static BlockProcessor getInstanse(BlockingQueue<BlockingSoft> blockingQ, UserAPI userAPI)
        {
            if (instanse == null)
            {
                instanse = new BlockProcessor(blockingQ, userAPI);
            }
            return instanse;
        }

        public void addBlockingSoft(List<BlockingSoft> doBlockingSoft)
        {
            blockingQ.clearAllAndPutCollection(doBlockingSoft);
        }

        private void blockingThreadFx()
        {
            while (true)
            {
                try
                {
                    foreach (BlockingSoft soft in blockingQ.getAll().ToArray())
                    {
                        foreach (Process proc in Process.GetProcessesByName(soft.getSoft()))
                        {
                            proc.Kill();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[BlockProcessor::blockingThreadFx(void)] Ошибка: " + e.Message);
                }
                finally
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void clean()
        {
            this.blockThreading.Abort();
        }
    }
}
