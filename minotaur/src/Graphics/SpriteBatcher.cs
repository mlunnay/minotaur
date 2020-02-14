using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Minotaur.Graphics
{
  /// <summary>
  /// This class handles the queueing of sprite batch items to be rendered to the GPU.
  /// </summary>
  internal class SpriteBatcher
  {
    /// <summary>
    /// Initialization size for the batch item list and queue.
    /// </summary>
    private const int InitalBatchSize = 256;

    private const int MaxBatchSize = short.MaxValue / 16; // lowered to short.MaxValue / 16 to handle sliced sprites  // 6 = 4 vertices unique and 2 shared per quad

    /// <summary>
    /// The list of batch items to process.
    /// </summary>
    private readonly List<SpriteBatchItem> _batchItems;

    /// <summary>
    /// the available SpriteBatchItems so that we can reuse these objects and avoid creation overhead.
    /// </summary>
    private readonly Queue<SpriteBatchItem> _freeBatchItems;

    /// <summary>
    /// Vertex index array.
    /// </summary>
    private ushort[] _index;

    private int _indexStart;

    /// <summary>
    /// The vertex array used for each call to draw a batch.
    /// </summary>
    private VertexPositionColorTexture[] _vertexArray;

    private GraphicsDevice _device;

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private VertexArray _voa;

    private Dictionary<string, IUniformValue> _shaderUniforms = new Dictionary<string, IUniformValue>();

    public SpriteBatcher(GraphicsDevice device)
    {
      _device = device;

      _batchItems = new List<SpriteBatchItem>(InitalBatchSize);
      _freeBatchItems = new Queue<SpriteBatchItem>(InitalBatchSize);

      EnsureArrayCapacity(InitalBatchSize);

      _vertexBuffer = new VertexBuffer(VertexFormat.PositionColorTexture, MaxBatchSize * VertexFormat.PositionColorTexture.Stride, BufferUsageHint.StreamDraw);
      _indexBuffer = new IndexBuffer(BufferUsageHint.StreamDraw, DrawElementsType.UnsignedShort, MaxBatchSize);
      _voa = new VertexArray(_vertexBuffer, _indexBuffer);
    }

    /// <summary>
    /// Retrieves a SpriteBatchItem from the internal cache or creates a new one if there are none in the cache.
    /// </summary>
    /// <returns></returns>
    public SpriteBatchItem CreateBatchItem()
    {
      SpriteBatchItem item;
      if (_freeBatchItems.Count > 0)
        item = _freeBatchItems.Dequeue();
      else
        item = new SpriteBatchItem();
      _batchItems.Add(item);
      return item;
    }

    private void EnsureArrayCapacity(int numBatchItems)
    {
      int neededCapacity = 30 * numBatchItems;
      if (_index != null && neededCapacity <= _index.Length)
        return; // quick return if we already have enough capacity.

      //ushort[] newIndex = new ushort[neededCapacity];
      //int start = 0;
      //if (_index != null)
      //{
      //  _index.CopyTo(newIndex, 0);
      //  start = _index.Length / 6;
      //}

      //for (int i = start; i < numBatchItems; i++)
      //{
      //  /*
      //   *  TL    TR
      //   *   0----1 0,1,2,3 = index offsets for vertex indices
      //   *   |   /| TL,TR,BL,BR are vertex references in SpriteBatchItem.
      //   *   |  / |
      //   *   | /  |
      //   *   |/   |
      //   *   2----3
      //   *  BL    BR
      //   */
      //  // triangle 1
      //  newIndex[i * 6 + 0] = (short)(i * 4);
      //  newIndex[i * 6 + 1] = (short)(i * 4 + 1);
      //  newIndex[i * 6 + 2] = (short)(i * 4 + 2);
      //  // triangle 2
      //  newIndex[i * 6 + 3] = (short)(i * 4 + 1);
      //  newIndex[i * 6 + 4] = (short)(i * 4 + 2);
      //  newIndex[i * 6 + 5] = (short)(i * 4 + 3);
      //}
      //_index = newIndex;
      _index = new ushort[neededCapacity];

      _vertexArray = new VertexPositionColorTexture[8 * numBatchItems];
    }

    /// <summary>
    /// Does a reference comparison of the underlying Texture objects for each given SpriteBatchItem.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>0 if the textures are equal, 1 otherwise.</returns>
    private static int CompareTexture(SpriteBatchItem a, SpriteBatchItem b)
    {
      return ReferenceEquals(a.Texture, b.Texture) ? 0 : 1;
    }

    /// <summary>
    /// Compares the Depth of two SpriteBatchItems.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static int CompareDepth(SpriteBatchItem a, SpriteBatchItem b)
    {
      return a.Depth.CompareTo(b.Depth);
    }

    /// <summary>
    /// Does a reverse comparison of the depth of two SpriteBatchItems.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static int CompareReverseDepth(SpriteBatchItem a, SpriteBatchItem b)
    {
      return b.Depth.CompareTo(a.Depth);
    }

    public void DrawBatch(SpriteSortMode sortMode, Program shader, Sampler sampler)
    {
      Comparison<SpriteBatchItem> sortFunction = CompareTexture;
      if (sortMode == SpriteSortMode.Texture)
        sortFunction = CompareTexture;
      else if (sortMode == SpriteSortMode.FrontToBack)
        sortFunction = CompareDepth;
      else if (sortMode == SpriteSortMode.BackToFront)
        sortFunction = CompareReverseDepth;
      DrawBatch(sortFunction, shader, sampler);
    }

    public void DrawBatch(Comparison<SpriteBatchItem> sortFunction, Program shader, Sampler sampler)
    {
      if (_batchItems.Count == 0)
        return; // no batch items to draw.

      // sort the batch items.
      _batchItems.Sort(sortFunction);
      _batchItems.Sort(sortFunction);
      // for some reson the above sort makes no change
      //List<SpriteBatchItem> batches = _batchItems;
      //batches.Sort(sortFunction);

      // Determine how many iterations through the drawing code we need to make.
      int batchIndex = 0;
      int batchCount = _batchItems.Count;
      // Iterate through the batches, doing short.MaxValue sets of verticies at a time.
      while (batchCount > 0)
      {
        int verticesIndex = 0;
        int indicesIndex = 0;
        Texture2D texture = null;

        int numBatchesToProcess = batchCount;
        if (numBatchesToProcess > MaxBatchSize)
          numBatchesToProcess = MaxBatchSize;
        EnsureArrayCapacity(numBatchesToProcess);

        // draw the batches.
        for (int i = 0; i < numBatchesToProcess; i++, batchIndex++)
        {
          SpriteBatchItem item = _batchItems[batchIndex];
          //SpriteBatchItem item = batches[batchIndex];
          // if the texture changed, we need to draw the current vertecies and bind the new texture.
          if (!ReferenceEquals(item.Texture, texture))
          {
            FlushVertexArray(indicesIndex, verticesIndex, shader);

            texture = item.Texture;
            verticesIndex = 0;
            indicesIndex = 0;
            shader.Bind();
            //sampler.Texture = texture;
            Sampler s = (Sampler)sampler.Clone();
            s.Texture = texture;
            _shaderUniforms["Texture"] = UniformValue.Create(s);
            shader.BindUniforms(_shaderUniforms);
            //Sampler s = (Sampler)sampler.Clone();
            //s.Texture = texture;
            //shader.BindUniforms(new Dictionary<string, IUniformValue>()
            //{
            //  {"Texture", UniformValue.Create(s)}
            //});
            shader.Unbind();
            //_device.Textures[0] = texture;
          }

          Vector4 color = new Vector4(item.Color.R, item.Color.G, item.Color.B, item.Color.A);
          ushort[] itemIndices;
          VertexPositionColorTexture[] itemVertieces = item.Sprite.GetVertices(item.X, item.Y, item.Width, item.Height, item.Depth, item.Color, out itemIndices);

          if (item.SpriteEffect != SpriteEffect.None)
          {
            for(int j = 0; j <  itemVertieces.Length; j++)
            {
              Vector2 tmp = itemVertieces[j].TextureCoordinate;
              if (item.SpriteEffect == SpriteEffect.FlipVertically)
                tmp.Y = 1.0f - tmp.Y;
              if (item.SpriteEffect == SpriteEffect.FlipHorizontally)
                tmp.X = 1.0f - tmp.X;
              itemVertieces[j].TextureCoordinate = tmp;
            }
          }

          for (int j = 0; j < itemIndices.Length; j++)
            itemIndices[j] = (ushort)(itemIndices[j] + verticesIndex);
          Array.Copy(itemIndices, 0, _index, indicesIndex, itemIndices.Length);
          indicesIndex += itemIndices.Length;
          Array.Copy(itemVertieces, 0, _vertexArray, verticesIndex, itemVertieces.Length);
          verticesIndex += itemVertieces.Length;

          // release the texture and return the item to the queue.
          item.Sprite = null;
          _freeBatchItems.Enqueue(item);
        }

        // flush the remaining vertex array
        FlushVertexArray(indicesIndex, verticesIndex, shader);

        // update the batch count to continue the process with larger batches
        batchCount -= numBatchesToProcess;
      }
      _batchItems.Clear();
    }

    private void FlushVertexArray(int indicesEnd, int verticesEnd, Program shader)
    {
      if (indicesEnd == 0)
        return;

      // TODO: Draw vertex to GPU
      //_vertexBuffer.SetData(_vertexArray, 0, verticesEnd, true);
      _vertexBuffer.SetData(_vertexArray.Take(verticesEnd).ToArray(), true);
      //_indexBuffer.SetData(_index, 0, indicesEnd, true);
      _indexBuffer.SetData(_index.Take(indicesEnd).ToArray(), true);

      _device.World = Matrix4.Identity;
      //TODO: move to GraphicsDevice so statistics can be collected
      //GL.DrawElements(_vertexBuffer.BeginMode, (verticesEnd / 4) * 2, DrawElementsType.UnsignedInt, _index);
      //_device.DrawVertexArray(_voa, 0, indicesEnd, shader);
      _device.DrawVertexArray(_voa, shader);
      shader.Unbind();
    }
  }
}
