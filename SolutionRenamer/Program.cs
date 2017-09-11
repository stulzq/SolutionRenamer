using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace SolutionRenamer
{
	static class Program
    {
        static void Main(string[] args)
        {

	        Console.Title = "SolutionRenamer";

			//加载配置
	        var builder = new ConfigurationBuilder()
		        .SetBasePath(Directory.GetCurrentDirectory())
		        .AddJsonFile("Config.json");

	        var configuration = builder.Build();

			var fileExtensions = configuration["FileExtension"];

	        string[] filter = fileExtensions.Split(',');

			Console.WriteLine();
			Console.WriteLine("Input your company name:");
	        var oldCompanyName = Console.ReadLine();

	        Console.WriteLine("Input your peoject name(AbpZeroTemplate):");
	        var oldPeojectName = Console.ReadLine();
	        if (string.IsNullOrEmpty(oldPeojectName))
	        {
		        oldPeojectName = "AbpZeroTemplate";
	        }


			Console.WriteLine("Input your new company name:");
	        var newCompanyName = Console.ReadLine();

	        Console.WriteLine("Input your new peoject name(NewAbpZeroTemplate):");
	        var newPeojectName = Console.ReadLine();
	        if (string.IsNullOrEmpty(newPeojectName))
	        {
		        newPeojectName = "NewAbpZeroTemplate";
	        }


			Console.WriteLine("Input folder:");
	        var rootDir = Console.ReadLine();

			Stopwatch sp=new Stopwatch();

	        long spdir, spfile = 0;

			sp.Start();
			RenameAllDir(rootDir,oldCompanyName, oldPeojectName,newCompanyName,newPeojectName);
			sp.Stop();
	        spdir = sp.ElapsedMilliseconds;
			Console.WriteLine("Directory rename complete! spend:" + sp.ElapsedMilliseconds);
			
			sp.Reset();
			sp.Start();
	        RenameAllFileNameAndContent(rootDir, oldCompanyName, oldPeojectName, newCompanyName, newPeojectName, filter);
	        sp.Stop();
	        spfile = sp.ElapsedMilliseconds;
			Console.WriteLine("Filename and content rename complete! spend:" + sp.ElapsedMilliseconds);

			Console.WriteLine("");
			Console.WriteLine("=====================================Report=====================================");
			Console.WriteLine($"Processing spend time,directories:{spdir},files:{spfile}");
			Console.ReadKey();
        }


		#region 递归重命名所有目录

		/// <summary>
		/// 递归重命名所有目录
		/// </summary>
		static void RenameAllDir(string rootDir,string oldCompanyName,string oldPeojectName,string newCompanyName,string newProjectName)
	    {
		    string[] allDir = Directory.GetDirectories(rootDir);

		    foreach (var item in allDir)
		    {
			    RenameAllDir(item, oldCompanyName,oldPeojectName,newCompanyName,newProjectName);

			    DirectoryInfo dinfo = new DirectoryInfo(item);
			    if (dinfo.Name.Contains(oldCompanyName)||dinfo.Name.Contains(oldPeojectName))
			    {
				    var newName = dinfo.Name;

					if (!string.IsNullOrEmpty(oldCompanyName))
					{
						newName = newName.Replace(oldCompanyName, newCompanyName);
					}
				    newName = newName.Replace(oldPeojectName,newProjectName);

					var newPath = Path.Combine(dinfo.Parent.FullName, newName);

				    if (dinfo.FullName != newPath)
				    {
						Console.WriteLine(dinfo.FullName);
					    Console.WriteLine("->");
					    Console.WriteLine(newPath);
					    Console.WriteLine("-------------------------------------------------------------");
					    dinfo.MoveTo(newPath);
					}
				    
			    }
		    }
	    }

		#endregion

		#region 递归重命名所有文件名和文件内容

		/// <summary>
		/// 递归重命名所有文件名和文件内容
		/// </summary>
		static void RenameAllFileNameAndContent(string rootDir, string oldCompanyName, string oldPeojectName, string newCompanyName, string newProjectName, string[] filter)
	    {
		    //获取当前目录所有指定文件扩展名的文件
		    List<FileInfo> files =new DirectoryInfo(rootDir).GetFiles().Where(m=> filter.Any(f => f == m.Extension)).ToList();
			
		    //重命名当前目录文件和文件内容
		    foreach (var item in files)
		    {

			    var text = File.ReadAllText(item.FullName, Encoding.UTF8);
			    if (!string.IsNullOrEmpty(oldCompanyName))
			    {
					text = text.Replace(oldCompanyName, newCompanyName);
				}
				
			    text = text.Replace(oldPeojectName, newProjectName);
			    if (item.Name.Contains(oldCompanyName)|| item.Name.Contains(oldPeojectName))
			    {
					var newName= item.Name;

				    if (!string.IsNullOrEmpty(oldCompanyName))
				    {
					    newName = newName.Replace(oldCompanyName, newCompanyName);

					}
				    newName = newName.Replace(oldPeojectName, newProjectName);
					var newFullName = Path.Combine(item.DirectoryName, newName);
				    File.WriteAllText(newFullName, text, Encoding.UTF8);
				    if (newFullName != item.FullName)
				    {
						File.Delete(item.FullName);
					}
				}
			    else
			    {
				    File.WriteAllText(item.FullName,text,Encoding.UTF8);
			    }
			    Console.WriteLine(item.Name+" process complete!");

		    }

		    //获取子目录
		    string[] dirs = Directory.GetDirectories(rootDir);
		    foreach (var dir in dirs)
		    {
				RenameAllFileNameAndContent(dir, oldCompanyName, oldPeojectName, newCompanyName, newProjectName, filter);
			}
	    }

	    #endregion
	}

}
