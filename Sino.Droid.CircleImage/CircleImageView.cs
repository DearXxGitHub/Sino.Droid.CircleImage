using System;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Android.Graphics;
using Android.Util;
using Android.Graphics.Drawables;
using Android.Content.Res;

namespace Sino.Droid.CircleImage
{
	public class CircleImageView : ImageView
	{
        private static ScaleType SCALE_TYPE = ScaleType.CenterCrop;

        private static Bitmap.Config BITMAP_CONFIG = Bitmap.Config.Argb8888;
        private static int COLORDRAWABLE_DIMENSION = 2;

        private static int DEFAULT_BORDER_WIDTH = 0;
        private static Color DEFAULT_BORDER_COLOR = Color.Black;
        private static bool DEFAULT_BORDER_OVERLAY = false;

        private RectF mDrawableRect = new RectF();
        private RectF mBorderRect = new RectF();

        private Matrix mShaderMatrix = new Matrix();
        private Paint mBitmapPaint = new Paint();
        private Paint mBorderPaint = new Paint();

        private Color mBorderColor = DEFAULT_BORDER_COLOR;
        private int mBorderWidth = DEFAULT_BORDER_WIDTH;

        private Bitmap mBitmap;
        private BitmapShader mBitmapShader;
        private int mBitmapWidth;
        private int mBitmapHieght;

        private float mDrawableRadius;
        private float mBorderRadius;

        private ColorFilter mColorFilter;

        private bool mReady;
        private bool mSetupPending;
        private bool mBorderOverlay;

        protected CircleImageView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public CircleImageView(Context context)
            : this(context, null, 0)
        {
            Init();
        }

        public CircleImageView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public CircleImageView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.Sino_Droid_CircleImageView, defStyle, 0);

            mBorderWidth = a.GetDimensionPixelSize(Resource.Styleable.Sino_Droid_CircleImageView_border_width, DEFAULT_BORDER_WIDTH);
            mBorderColor = a.GetColor(Resource.Styleable.Sino_Droid_CircleImageView_border_color, DEFAULT_BORDER_COLOR);
            mBorderOverlay = a.GetBoolean(Resource.Styleable.Sino_Droid_CircleImageView_border_overlay, DEFAULT_BORDER_OVERLAY);

            a.Recycle();

            Init();
        }

        private void Init()
        {
            base.SetScaleType(SCALE_TYPE);
            mReady = true;

            if (mSetupPending)
            {
                Setup();
                mSetupPending = false;
            }
        }

        public override ImageView.ScaleType GetScaleType()
        {
            return SCALE_TYPE;
        }

        public override void SetScaleType(ImageView.ScaleType scaleType)
        {
            if (scaleType != SCALE_TYPE)
            {
                throw new InvalidOperationException(String.Format("ScaleType {0} is not support", scaleType));
            }
        }

        public override void SetAdjustViewBounds(bool adjustViewBounds)
        {
            if (adjustViewBounds)
            {
                throw new InvalidOperationException("adjustViewBounds not supported.");
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (Drawable == null)
            {
                return;
            }

            canvas.DrawCircle(Width / 2, Height / 2, mDrawableRadius, mBitmapPaint);
            if (mBorderWidth != 0)
            {
                canvas.DrawCircle(Width / 2, Height / 2, mBorderRadius, mBorderPaint);
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            Setup();
        }

        public Color BorderColor
        {
            get
            {
                return mBorderColor;
            }
            set
            {
                if (value == mBorderColor)
                {
                    return;
                }
                mBorderColor = value;
                mBorderPaint.Color = mBorderColor;
                Invalidate();
            }
        }

        public void SetBorderColorResource(int borderColorRes)
        {
            BorderColor = Context.Resources.GetColor(borderColorRes);
        }

        public int BorderWidth
        {
            get
            {
                return mBorderWidth;
            }
            set
            {
                if (value == mBorderWidth)
                {
                    return;
                }
                mBorderWidth = value;
                Setup();
            }
        }

        public bool IsBorderOverlay()
        {
            return mBorderOverlay;
        }

        public void SetBorderOverlay(bool borderOverlay)
        {
            if (borderOverlay == mBorderOverlay)
            {
                return;
            }

            mBorderOverlay = borderOverlay;
            Setup();
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            base.SetImageBitmap(bm);
            mBitmap = bm;
            Setup();
        }

        public override void SetImageDrawable(Drawable drawable)
        {
            base.SetImageDrawable(drawable);
            mBitmap = GetBitmapFromDrawable(drawable);
            Setup();
        }

        public override void SetImageResource(int resId)
        {
            base.SetImageResource(resId);
            mBitmap = GetBitmapFromDrawable(Drawable);
            Setup();
        }

        public override void SetImageURI(Android.Net.Uri uri)
        {
            base.SetImageURI(uri);
            mBitmap = GetBitmapFromDrawable(Drawable);
            Setup();
        }

        public override void SetColorFilter(ColorFilter cf)
        {
            if (cf == mColorFilter)
            {
                return;
            }

            mColorFilter = cf;
            mBitmapPaint.SetColorFilter(mColorFilter);
            Invalidate();
        }

        private Bitmap GetBitmapFromDrawable(Drawable drawable)
        {
            if (drawable == null)
            {
                return null;
            }

            if (drawable is BitmapDrawable)
            {
                return ((BitmapDrawable)drawable).Bitmap;
            }

            try
            {
                Bitmap bitmap;

                if (drawable is ColorDrawable)
                {
                    bitmap = Bitmap.CreateBitmap(COLORDRAWABLE_DIMENSION, COLORDRAWABLE_DIMENSION, BITMAP_CONFIG);
                }
                else
                {
                    bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, BITMAP_CONFIG);
                }

                Canvas canvas = new Canvas(bitmap);
                drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
                drawable.Draw(canvas);
                return bitmap;
            }
            catch (OutOfMemoryException)
            {
                return null;
            }
        }

        private void Setup()
        {
            if (!mReady)
            {
                mSetupPending = true;
                return;
            }

            if (mBitmap == null)
            {
                return;
            }

            mBitmapShader = new BitmapShader(mBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);

            mBitmapPaint.AntiAlias = true;
            mBitmapPaint.SetShader(mBitmapShader);

            mBorderPaint.SetStyle(Paint.Style.Stroke);
            mBorderPaint.AntiAlias = true;
            mBorderPaint.Color = mBorderColor;
            mBorderPaint.StrokeWidth = mBorderWidth;

            mBitmapHieght = mBitmap.Height;
            mBitmapWidth = mBitmap.Width;

            mBorderRect.Set(0, 0, Width, Height);
            mBorderRadius = Math.Min((mBorderRect.Height() - mBorderWidth) / 2, (mBorderRect.Width() - mBorderWidth) / 2);

            mDrawableRect.Set(mBorderRect);
            if (!mBorderOverlay)
            {
                mDrawableRect.Inset(mBorderWidth, mBorderWidth);
            }
            mDrawableRadius = Math.Min(mDrawableRect.Height() / 2, mDrawableRect.Width() / 2);

            UpdateShaderMatrix();
            Invalidate();
        }

        private void UpdateShaderMatrix()
        {
            float scale;
            float dx = 0;
            float dy = 0;

            mShaderMatrix.Set(null);

            if (mBitmapWidth * mDrawableRect.Height() > mDrawableRect.Width() * mBitmapHieght)
            {
                scale = mDrawableRect.Height() / (float)mBitmapHieght;
                dx = (mDrawableRect.Width() - mBitmapWidth * scale) * 0.5f;
            }
            else
            {
                scale = mDrawableRect.Width() / (float)mBitmapWidth;
                dy = (mDrawableRect.Height() - mBitmapHieght * scale) * 0.5f;
            }

            mShaderMatrix.SetScale(scale, scale);
            mShaderMatrix.PostTranslate((int)(dx + 0.5f) + mDrawableRect.Left, (int)(dy + 0.5f) + mDrawableRect.Top);

            mBitmapShader.SetLocalMatrix(mShaderMatrix);
        }
    }
}