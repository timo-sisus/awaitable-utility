#if UNITY_2023_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sisus
{
	/// <summary>
	/// Utility methods related to <see cref="Awaitable"/> and <see cref="Awaitable{TResult}"/>.
	/// <para>
	/// Contains methods for creating awaitables that have already completed, failed or been cancelled,
	/// as well as methods for waiting for multiple awaitables to complete.
	/// </para>
	/// </summary>
	public static class AwaitableUtility
	{
		static readonly AwaitableCompletionSource completionSource = new();

		/// <summary>
		/// Gets an <see cref="Awaitable"/> that has already <see cref="Awaitable.IsCompleted">completed</see>.
		/// </summary>
		public static Awaitable CompletedAwaitable
		{
			get
			{
				completionSource.SetResult();
				var awaitable = completionSource.Awaitable;
				completionSource.Reset();
				return awaitable;
			}
		}

		/// <summary>
		/// Gets an <see cref="Awaitable"/> that has already failed with the specified exception.
		/// </summary>
		/// <param name="exception"> The exception to store into the completed awaitable. </param>
		/// <returns> A faulted awaitable. </returns>
		public static Awaitable FromException(Exception exception)
		{
			completionSource.SetException(exception);
			var awaitable = completionSource.Awaitable;
			completionSource.Reset();
			return awaitable;
		}

		/// <summary>
		/// Gets an <see cref="Awaitable"/> that has already been cancelled.
		/// </summary>
		/// <returns> A cancelled awaitable. </returns>
		public static Awaitable FromCanceled()
		{
			completionSource.SetCanceled();
			var awaitable = completionSource.Awaitable;
			completionSource.Reset();
			return awaitable;
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{TResult}"/> that has already completed with the specified result.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result returned by the awaitable. </typeparam>
		/// <param name="result"> The result to store into the completed awaitable. </param>
		/// <returns> A completed awaitable. </returns>
		public static Awaitable<TResult> FromResult<TResult>(TResult result) => Result<TResult>.From(result);

		/// <summary>
		/// Gets an <see cref="Awaitable{TResult}"/> that has already completed with the specified result.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result returned by the awaitable. </typeparam>
		/// <param name="result"> The result to store into the completed awaitable. </param>
		/// <returns> A completed awaitable. </returns>
		public static Awaitable<TResult> FromException<TResult>(Exception exception) => Result<TResult>.From(exception);

		/// <summary>
		/// Gets an <see cref="Awaitable{TResult}"/> that has already completed with the specified result.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result returned by the awaitable. </typeparam>
		/// <param name="result"> The result to store into the completed awaitable. </param>
		/// <returns> A completed awaitable. </returns>
		public static Awaitable<TResult> FromCanceled<TResult>() => Result<TResult>.Canceled();

		#region WhenAll_No_Result
		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable WhenAll(Awaitable awaitable1, Awaitable awaitable2)
		{
			try
			{
				await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable2;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable WhenAll(Awaitable awaitable1, Awaitable awaitable2, Awaitable awaitable3)
		{
			try
			{
				await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable3;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}
		}

		#endregion
		#region WhenAll_One_Result
		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result produced by the first awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<TResult> WhenAll<TResult>(Awaitable<TResult> awaitable1, Awaitable awaitable2)
		{
			TResult result;
			try
			{
				result = await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable2;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return result;
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result produced by the second awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<TResult> WhenAll<TResult>(Awaitable awaitable1, Awaitable<TResult> awaitable2)
		{
			try
			{
				await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult result;
			try
			{
				result = await awaitable2;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return result;
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result produced by the first awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<TResult> WhenAll<TResult>(Awaitable<TResult> awaitable1, Awaitable awaitable2, Awaitable awaitable3)
		{
			TResult result;
			try
			{
				result = await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable3;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return result;
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result produced by the second awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<TResult> WhenAll<TResult>(Awaitable awaitable1, Awaitable<TResult> awaitable2, Awaitable awaitable3)
		{
			try
			{
				await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult result;
			try
			{
				result = await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable3;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return result;
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult"> The type of the result produced by the third awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<TResult> WhenAll<TResult>(Awaitable awaitable1, Awaitable awaitable2, Awaitable<TResult> awaitable3)
		{
			try
			{
				await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				return await awaitable3;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}
		}

		#endregion
		#region WhenAll_Two_Results
		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<(TResult1 result1, TResult2 result2)> WhenAll<TResult1, TResult2>(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2)
		{
			TResult1 result1;
			try
			{
				result1 = await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult2 result2;
			try
			{
				result2 = await awaitable2;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return (result1, result2);
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<(TResult1 result1, TResult2 result2)> WhenAll<TResult1, TResult2> (Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable awaitable3)
		{
			TResult1 result1;
			try
			{
				result1 = await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult2 result2;
			try
			{
				result2 = await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable3;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return (result1, result2);
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<(TResult1 result1, TResult2 result2)> WhenAll<TResult1, TResult2> (Awaitable<TResult1> awaitable1, Awaitable awaitable2, Awaitable<TResult2> awaitable3)
		{
			TResult1 result1;
			try
			{
				result1 = await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			try
			{
				await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult2 result2;
			try
			{
				result2 = await awaitable3;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return (result1, result2);
		}

		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<(TResult1 result1, TResult2 result2)> WhenAll<TResult1, TResult2> (Awaitable awaitable1, Awaitable<TResult1> awaitable2, Awaitable<TResult2> awaitable3)
		{
			try
			{
				await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult1 result1;
			try
			{
				result1 = await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult2 result2;
			try
			{
				result2 = await awaitable3;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return (result1, result2);
		}
		#endregion
		#region WhenAll_Three_Results
		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <typeparam name="TResult3"> The type of the result produced by the third awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3)> WhenAll<TResult1, TResult2, TResult3>(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3)
		{
			TResult1 result1;
			try
			{
				result1 = await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult2 result2;
			try
			{
				result2 = await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult3 result3;
			try
			{
				result3 = await awaitable3;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return (result1, result2, result3);
		}
		#endregion
		#region WhenAll_Four_Results
		/// <summary>
		/// Gets an <see cref="Awaitable{}"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <typeparam name="TResult1"> The type of the result produced by the first awaitable. </typeparam>
		/// <typeparam name="TResult2"> The type of the result produced by the second awaitable. </typeparam>
		/// <typeparam name="TResult3"> The type of the result produced by the third awaitable. </typeparam>
		/// <typeparam name="TResult4"> The type of the result produced by the fourth awaitable. </typeparam>
		/// <param name="awaitable1"> The first awaitable to wait on for completion. </param>
		/// <param name="awaitable2"> The second awaitable to wait on for completion. </param>
		/// <param name="awaitable3"> The third awaitable to wait on for completion. </param>
		/// <param name="awaitable4"> The fourth awaitable to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable<(TResult1 result1, TResult2 result2, TResult3 result3, TResult4 result4)>
			WhenAll<TResult1, TResult2, TResult3, TResult4>
			(Awaitable<TResult1> awaitable1, Awaitable<TResult2> awaitable2, Awaitable<TResult3> awaitable3, Awaitable<TResult4> awaitable4)
		{
			TResult1 result1;
			try
			{
				result1 = await awaitable1;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable2;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable3;
					}
					catch(Exception thirdException)
					{
						try
						{
							await awaitable4;
						}
						catch(Exception fourthException)
						{
							throw CreateAggregateException(firstException, secondException, thirdException, fourthException);
						}

						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult2 result2;
			try
			{
				result2 = await awaitable2;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable3;
				}
				catch(Exception secondException)
				{
					try
					{
						await awaitable4;
					}
					catch(Exception thirdException)
					{
						throw CreateAggregateException(firstException, secondException, thirdException);
					}

					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult3 result3;
			try
			{
				result3 = await awaitable3;
			}
			catch(Exception firstException)
			{
				try
				{
					await awaitable4;
				}
				catch(Exception secondException)
				{
					throw CreateAggregateException(firstException, secondException);
				}

				throw CreateAggregateException(firstException);
			}

			TResult4 result4;
			try
			{
				result4 = await awaitable4;
			}
			catch(Exception onlyException)
			{
				throw CreateAggregateException(onlyException);
			}

			return (result1, result2, result3, result4);
		}
		#endregion
		#region WhenAll_Any_Results
		/// <summary>
		/// Gets an <see cref="Awaitable"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <param name="awaitables"> The awaitables to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable WhenAll(Awaitable[] awaitables)
		{
			var count = awaitables.Length;
			var exceptions = Array.Empty<Exception>();
			var exceptionCount = 0;
			for(int i = 0; i < count; i++)
			{
				var awaitable = awaitables[i];
				try
				{
					await awaitable;
				}
				catch(Exception exception)
				{
					exceptionCount++;
					Array.Resize(ref exceptions, exceptionCount);
					exceptions[exceptionCount - 1] = exception;
				}
			}

			if(exceptionCount > 0)
			{
				throw CreateAggregateException(exceptions);
			}
		}

		/// <summary>
		/// Gets an <see cref="Awaitable"/> that will complete when all the supplied awaitables have completed.
		/// </summary>
		/// <param name="awaitables"> The awaitables to wait on for completion. </param>
		/// <returns>
		/// An awaitable that represents the completion of all the supplied awaitables.
		/// </returns>
		public static async Awaitable WhenAll(IEnumerable<Awaitable> awaitables)
		{
			var exceptions = Array.Empty<Exception>();
			var exceptionCount = 0;
			foreach(var awaitable in awaitables)
			{
				try
				{
					await awaitable;
				}
				catch(Exception exception)
				{
					exceptionCount++;
					Array.Resize(ref exceptions, exceptionCount);
					exceptions[exceptionCount - 1] = exception;
				}
			}

			if(exceptionCount > 0)
			{
				throw CreateAggregateException(exceptions);
			}
		}
		#endregion

		/// <summary>
		/// Wrap the <see paramref="exception"/> inside an <see cref="AggregateException"/> which we can throw without
		/// causing the stack trace of the original exception to be lost.
		/// </summary>
		/// <param name="exception"> Exception to wrap. </param>
		/// <returns>
		/// An <see cref="AggregateException"/> that wraps the original exception, or the original exception if it was
		/// already an <see cref="AggregateException"/>.
		/// </returns>
		/// <exception cref="Exception"></exception>
		static Exception CreateAggregateException(Exception exception) => throw (exception is AggregateException ? exception : new AggregateException(exception));

		/// <summary>
		/// Wrap the <see paramref="exceptions"/> inside an <see cref="AggregateException"/> which we can throw without
		/// causing the stack trace of the original exceptions to be lost.
		/// </summary>
		/// <param name="exceptions"> Exceptions to wrap. </param>
		/// <returns>
		/// An <see cref="AggregateException"/> that wraps the original exceptions
		/// </returns>
		static Exception CreateAggregateException(params Exception[] exceptions)
		{
			if(!exceptions.Any(e => e is OperationCanceledException))
			{
				return new AggregateException(exceptions).Flatten();
			}

			if(exceptions.All(e => e is OperationCanceledException))
			{
				return new AggregateException(exceptions[0]);
			}

			return new AggregateException(exceptions.Where(e => e is not OperationCanceledException)).Flatten();
		}

		static class Result<TResult>
		{
			static readonly AwaitableCompletionSource<TResult> completionSource = new();

			internal static Awaitable<TResult> From(TResult result)
			{
				completionSource.SetResult(result);
				var awaitable = completionSource.Awaitable;
				completionSource.Reset();
				return awaitable;
			}

			internal static Awaitable<TResult> From(Exception exception)
			{
				completionSource.SetException(exception);
				var awaitable = completionSource.Awaitable;
				completionSource.Reset();
				return awaitable;
			}

			internal static Awaitable<TResult> Canceled()
			{
				completionSource.SetCanceled();
				var awaitable = completionSource.Awaitable;
				completionSource.Reset();
				return awaitable;
			}
		}
	}
}
#endif
