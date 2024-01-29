using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Model_Bank
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dataGridView1.RowHeadersVisible = false;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            // Входные параметры с формы
            int minTellers = int.Parse(textBox1.Text); // Минимальное количество кассиров
            int maxTellers = int.Parse(textBox2.Text); // Максимальное количество кассиров
            double meanInterarrival = Convert.ToDouble(textBox3.Text); // Среднее время между приходом клиентов
            double meanService = Convert.ToDouble(textBox4.Text); // Среднее время обслуживания клиентов
            double sigmaService = Convert.ToDouble(textBox7.Text); // Ср. кв. отклонение времени обслуживания клиентов
            int timeBank = int.Parse(textBox5.Text); // Длительность работы банка в часах
            int N = int.Parse(textBox6.Text); // Количество розыгрышей

            // Параметры для моделирования
            double averageClientQueues; // среднее число клиентов в очереди
            int totalClientQueues;      // число клиентов в очереди
            int sumClientQueues;    // сумма клиентов в очереди за итерации (дни)
            double averageMax;      // среднее по максимуму
            double averageMin;      // среднее по минимуму
            int totalClient;        // общее количество клиентов
            double averageTimeQueues;   // среднее время в очереди
            double maxQueues;
            double minQueues;
            double averageCassaWorkTime; // среднее время работы касс
            double coefLoad;        // коэффициент загруженности касс

            double intervalTime;    // время прибытия клиента
            double serviceTime;     // время обслуживания клиента
            double queueTime;       // время в очереди
            double time;            // общее время

            List<Cassa> cassaTime = new List<Cassa>();

            dataGridView1.RowCount = maxTellers - minTellers + 1;

            // Моделирование
            for (int casseNumber = minTellers; casseNumber <= maxTellers; casseNumber++)
            {
                for (int i = 0; i < casseNumber; i++) // Добавляем время обслуживания и работы
                                                      // каждой кассы в список
                {
                    Cassa cassa = new Cassa();
                    cassaTime.Add(cassa);
                }

                averageClientQueues = 0;
                sumClientQueues = 0;
                averageMax = 0;
                averageMin = 0;
                averageTimeQueues = 0;
                averageCassaWorkTime = 0;

                for (int i = 0; i < N; i++)
                {
                    totalClientQueues = 0;
                    totalClient = 0;
                    queueTime = 0;
                    coefLoad = 0;

                    Generator generator = new Generator((1.0 / meanInterarrival) * 0.5); // инверсия среднего значения с учетом коэффициента
                    intervalTime = generator.randomize();

                    {
                        // Random random = new Random();
                        // double u1 = random.NextDouble();
                        // double u2 = random.NextDouble();
                        // serviceTime = meanService + sigmaService * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);

                        double min = meanService - sigmaService;
                        double max = meanService + sigmaService;  
                        do
                        {
                            serviceTime = generator.NextGaussian(meanService, sigmaService);
                        }
                        while (serviceTime < min || serviceTime > max);
                    }

                    time = 0;

                    for (int k = 0; k < casseNumber; k++)
                    {
                        cassaTime[k].serviceTime = 0;
                        cassaTime[k].workTime = 0;
                    }

                    maxQueues = 0;
                    minQueues = 10000;
                    while (((time + intervalTime) < (timeBank * 60))) //& (cassaTime.Max(c => c.workTime) + serviceTime <= (timeBank * 60)))
                                                                    // (cassaTime.Max(c => c.workTime) + serviceTime <= (timeBank * 60))
                    {
                        time += intervalTime;
                        totalClient++;

                        double minCassaServiceTime = cassaTime.Min(c => c.serviceTime);
                        int index = cassaTime.FindIndex(c => c.serviceTime == minCassaServiceTime);

                        cassaTime[index].workTime += serviceTime;

                        if (time < minCassaServiceTime)
                        {                   
                            queueTime += minCassaServiceTime - time;
                            totalClientQueues++;
                            
                            cassaTime[index].serviceTime += serviceTime;

                            if (maxQueues <= (minCassaServiceTime - time))
                            {
                                maxQueues = minCassaServiceTime - time;
                            }
                        
                            if (minQueues >= (minCassaServiceTime - time))
                            {
                                minQueues = minCassaServiceTime - time;
                            }
                        } 
                        else
                        {
                            cassaTime[index].serviceTime += (time + serviceTime);
                        }
                    }

                    for (int ii = 0; ii < casseNumber; ii++)
                    {
                        coefLoad += cassaTime[ii].workTime/(timeBank*60);
                    }
                    
                    averageMax += maxQueues;
                    averageMin += minQueues;
                    averageCassaWorkTime += coefLoad/casseNumber;

                    averageTimeQueues += queueTime/ totalClientQueues;
                    averageClientQueues += totalClientQueues / serviceTime;
                    sumClientQueues += totalClientQueues;
                    //coefLoad += totalClient/casseNumber;
                }
                averageMax /= N;
                averageMin /= N;
                averageClientQueues /= N;
                sumClientQueues /= N;
                averageTimeQueues /= N;
                averageCassaWorkTime /= N;
                //coefLoad /= N;

                cassaTime.Clear();

                // Вывод
                dataGridView1.Rows[casseNumber - minTellers].Cells[0].Value = casseNumber;  // число касс

                if (double.IsNaN(averageTimeQueues))
                {
                    dataGridView1.Rows[casseNumber - minTellers].Cells[1].Value = 0; // среднее число клиентов в очереди
                    dataGridView1.Rows[casseNumber - minTellers].Cells[2].Value = 0; // количество клиентов в очереди
                    dataGridView1.Rows[casseNumber - minTellers].Cells[3].Value = "очереди нет"; // среднее значение задержки в очереди
                    dataGridView1.Rows[casseNumber - minTellers].Cells[4].Value = 0;    // максимальная задержка в очереди
                    dataGridView1.Rows[casseNumber - minTellers].Cells[5].Value = 0;    // минимальная задержка в очереди
                }
                else
                {
                    dataGridView1.Rows[casseNumber - minTellers].Cells[1].Value = Math.Round(averageClientQueues, 0);// среднее число клиентов в очереди
                    dataGridView1.Rows[casseNumber - minTellers].Cells[2].Value = sumClientQueues; // количество клиентов в очереди
                    dataGridView1.Rows[casseNumber - minTellers].Cells[3].Value = Math.Round(averageTimeQueues, 2); // среднее значение задержки в очереди
                    dataGridView1.Rows[casseNumber - minTellers].Cells[4].Value = Math.Round(averageMax, 2);    // максимальная задержка в очереди
                    dataGridView1.Rows[casseNumber - minTellers].Cells[5].Value = Math.Round(averageMin, 2);    // минимальная задержка в очереди
                }

                dataGridView1.Rows[casseNumber - minTellers].Cells[6].Value = Math.Round(averageCassaWorkTime, 3);  // коэффициент загруженности касс
            }
        }
    }
}