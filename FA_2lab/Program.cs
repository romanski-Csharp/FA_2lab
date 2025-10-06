namespace FA_2lab
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            double[,] A = {
                { 11, 5, -5 },
                { 3, -7, -2 },
                { 1, -1,  4 }
            };

            double[] b = { 13.5, 4, -1 };
            double epsilon = 0.001;

            Console.WriteLine("Вихідна система рівнянь:");
            Console.WriteLine("x₁ - x₂ + 4x₃ = -1");
            Console.WriteLine("3x₁ - 7x₂ - 2x₃ = 4");
            Console.WriteLine("11x₁ + 5x₂ - 5x₃ = 13.5\n");

            PrintMatrix("Матриця A:", A);
            PrintVector("Вектор b:", b);

            SimpleIterationSolver solver = new SimpleIterationSolver();
            double[] solution = solver.Solve(A, b, epsilon);

            if (solution != null)
            {
                Console.WriteLine("\n=== РОЗВ'ЯЗОК ===");
                Console.WriteLine($"x₁ = {solution[0]:F6}");
                Console.WriteLine($"x₂ = {solution[1]:F6}");
                Console.WriteLine($"x₃ = {solution[2]:F6}");

                // Перевірка розв'язку
                VerifySolution(A, b, solution);
            }
            else
            {
                Console.WriteLine("\n❌ Метод не збігся або досягнута максимальна кількість ітерацій");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для завершення...");
            Console.ReadKey();
        }

        static void PrintMatrix(string title, double[,] matrix)
        {
            Console.WriteLine(title);
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                Console.Write("│ ");
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{matrix[i, j],8:F2} ");
                }
                Console.WriteLine("│");
            }
            Console.WriteLine();
        }

        static void PrintVector(string title, double[] vector)
        {
            Console.WriteLine(title);
            Console.Write("│ ");
            for (int i = 0; i < vector.Length; i++)
            {
                Console.Write($"{vector[i],8:F2} ");
            }
            Console.WriteLine("│\n");
        }

        static void VerifySolution(double[,] A, double[] b, double[] x)
        {
            Console.WriteLine("\n=== ПЕРЕВІРКА РОЗВ'ЯЗКУ ===");
            int n = x.Length;
            double[] result = new double[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = 0;
                for (int j = 0; j < n; j++)
                {
                    result[i] += A[i, j] * x[j];
                }
            }

            Console.WriteLine("A * x =");
            PrintVector("", result);

            Console.WriteLine("Нев'язка (A*x - b):");
            double maxError = 0;
            for (int i = 0; i < n; i++)
            {
                double error = Math.Abs(result[i] - b[i]);
                Console.WriteLine($"│ {result[i] - b[i],8:F6} │");
                if (error > maxError) maxError = error;
            }

            Console.WriteLine($"\nМаксимальна похибка: {maxError:F8}");
            if (maxError < 0.01)
                Console.WriteLine("✅ Розв'язок правильний!");
            else
                Console.WriteLine("❌ Велика нев'язка - перевірте розв'язок");
        }
    }

    class SimpleIterationSolver
    {
        public double[] Solve(double[,] A, double[] b, double epsilon)
        {
            int n = A.GetLength(0);
            Console.WriteLine("\n=== КРОК 1: ПРИВЕДЕННЯ ДО ІТЕРАЦІЙНОГО ВИГЛЯДУ ===");

            double[,] C = new double[n, n];
            double[] d = new double[n];

            CreateIterationForm(A, b, C, d);

            Console.WriteLine("\n=== КРОК 2: ПЕРЕВІРКА УМОВИ ЗБІЖНОСТІ ===");
            bool converges = CheckConvergenceCondition(C);

            if (!converges)
            {
                Console.WriteLine("\n=== НОРМУВАННЯ СИСТЕМИ ===");
                double[,] A_normalized = new double[n, n];
                double[] b_normalized = new double[n];

                NormalizeSystem(A, b, A_normalized, b_normalized);
                CreateIterationForm(A_normalized, b_normalized, C, d);

                Console.WriteLine("Перевірка умови збіжності після нормування:");
                CheckConvergenceCondition(C);
            }

            Console.WriteLine("\n=== КРОК 3: ІТЕРАЦІЙНИЙ ПРОЦЕС ===");
            return IterativeProcess(C, d, epsilon);
        }

        private void CreateIterationForm(double[,] A, double[] b, double[,] C, double[] d)
        {
            int n = A.GetLength(0);

            Console.WriteLine("Створення ітераційної форми x = Cx + d");

            for (int i = 0; i < n; i++)
            {
                if (Math.Abs(A[i, i]) < 1e-10)
                {
                    Console.WriteLine($"❌ ПОМИЛКА: Діагональний елемент A[{i + 1},{i + 1}] = {A[i, i]} занадто малий!");
                    return;
                }

                d[i] = b[i] / A[i, i];
                Console.Write($"x₄ = ");

                bool first = true;
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        C[i, j] = -A[i, j] / A[i, i];
                        if (!first && C[i, j] >= 0) Console.Write(" + ");
                        else if (!first) Console.Write(" ");

                        if (Math.Abs(C[i, j] - 1) < 1e-10)
                            Console.Write($"x₄");
                        else if (Math.Abs(C[i, j] + 1) < 1e-10)
                            Console.Write($"-x₄");
                        else
                            Console.Write($"{C[i, j]:F3}x₄");

                        first = false;
                    }
                    else
                    {
                        C[i, j] = 0;
                    }
                }

                if (d[i] >= 0 && !first) Console.Write(" + ");
                else if (!first) Console.Write(" ");
                Console.WriteLine($"{d[i]:F3}");
            }

            PrintMatrix("\nМатриця C:", C);
            PrintVector("Вектор d:", d);
        }

        private bool CheckConvergenceCondition(double[,] C)
        {
            int n = C.GetLength(0);
            bool converges = true;
            double maxRowSum = 0;

            Console.WriteLine("Перевірка умови: Σ|cᵢⱼ| ≤ α < 1 для кожного рядка i");

            for (int i = 0; i < n; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < n; j++)
                {
                    rowSum += Math.Abs(C[i, j]);
                }

                Console.WriteLine($"Рядок {i + 1}: Σ|c₄ⱼ| = {rowSum:F4} {(rowSum < 1 ? "✓" : "✗")}");

                if (rowSum >= 1) converges = false;
                if (rowSum > maxRowSum) maxRowSum = rowSum;
            }

            Console.WriteLine($"Максимальна сума: {maxRowSum:F4}");

            if (converges)
            {
                Console.WriteLine("✅ Умова збіжності виконується");
            }
            else
            {
                Console.WriteLine("❌ Умова збіжності НЕ виконується!");
                Console.WriteLine("Метод може не збігатися, але спробуємо...");
            }

            return converges;
        }

        private void NormalizeSystem(double[,] A, double[] b, double[,] A_norm, double[] b_norm)
        {
            int n = A.GetLength(0);

            Console.WriteLine("Виконуємо нормування: A^T * A * x = A^T * b");

            
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    A_norm[i, j] = 0;
                    for (int k = 0; k < n; k++)
                    {
                        A_norm[i, j] += A[k, i] * A[k, j]; // A^T * A
                    }
                }
            }

            
            for (int i = 0; i < n; i++)
            {
                b_norm[i] = 0;
                for (int k = 0; k < n; k++)
                {
                    b_norm[i] += A[k, i] * b[k]; // A^T * b
                }
            }

            PrintMatrix("Нормована матриця A^T*A:", A_norm);
            PrintVector("Нормований вектор A^T*b:", b_norm);
        }

        private double[] IterativeProcess(double[,] C, double[] d, double epsilon)
        {
            int n = C.GetLength(0);
            int maxIterations = 1000;

            double[] x = new double[n]; // Початкове наближення (0, 0, 0)
            double[] x_new = new double[n];

            Console.WriteLine($"Початкове наближення: x₀ = ({x[0]:F3}, {x[1]:F3}, {x[2]:F3})");
            Console.WriteLine($"Задана точність: ε = {epsilon}");
            Console.WriteLine();

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                for (int i = 0; i < n; i++)
                {
                    x_new[i] = d[i];
                    for (int j = 0; j < n; j++)
                    {
                        x_new[i] += C[i, j] * x[j];
                    }
                }

                double error = 0;
                for (int i = 0; i < n; i++)
                {
                    error += Math.Pow(x_new[i] - x[i], 2);
                }
                error = Math.Sqrt(error);

                if (iteration < 5 || iteration % 10 == 9 || error < epsilon)
                {
                    Console.WriteLine($"Ітерація {iteration + 1,3}: x = ({x_new[0],8:F6}, {x_new[1],8:F6}, {x_new[2],8:F6}), " +
                                    $"похибка = {error:F8}");
                }
                else if (iteration == 5)
                {
                    Console.WriteLine("...");
                }

                if (error < epsilon)
                {
                    Console.WriteLine($"\n✅ ЗБІЖНІСТЬ ДОСЯГНУТА за {iteration + 1} ітерацій");
                    Console.WriteLine($"Остаточна похибка: {error:F8}");
                    return x_new;
                }

                if (error > 1e6)
                {
                    Console.WriteLine($"\n❌ МЕТОД РОЗБІГАЄТЬСЯ (похибка = {error:E2})");
                    return null;
                }

                Array.Copy(x_new, x, n);
            }

            Console.WriteLine($"\n⚠️ Досягнута максимальна кількість ітерацій ({maxIterations})");
            Console.WriteLine($"Остання похибка: {Math.Sqrt(Math.Pow(x_new[0] - x[0], 2) + Math.Pow(x_new[1] - x[1], 2) + Math.Pow(x_new[2] - x[2], 2)):F8}");
            return x_new;
        }

        private void PrintMatrix(string title, double[,] matrix)
        {
            Console.WriteLine(title);
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                Console.Write("│ ");
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{matrix[i, j],10:F4} ");
                }
                Console.WriteLine("│");
            }
            Console.WriteLine();
        }

        private void PrintVector(string title, double[] vector)
        {
            Console.WriteLine(title);
            Console.Write("│ ");
            for (int i = 0; i < vector.Length; i++)
            {
                Console.Write($"{vector[i],10:F4} ");
            }
            Console.WriteLine("│\n");
        }
    }
}

