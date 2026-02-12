-- ==========================================
-- SUZUBUN DATABASE SCHEMA (POSTGRESQL)
-- Timezone: GMT+7 (ICT)
-- IDs: UUID
-- ==========================================

-- 1. Bảng Profiles (Lưu thông tin bổ sung cho User từ Supabase Auth)
CREATE TABLE IF NOT EXISTS public.profiles (
    id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    full_name TEXT,
    avatar_url TEXT,
    role TEXT DEFAULT 'user' CHECK (role IN ('admin', 'user')),
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'ict'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'ict')
);

-- 2. Hàm và Trigger tự động đồng bộ khi có User mới đăng ký
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO public.profiles (id, full_name, avatar_url, role)
  VALUES (
    NEW.id, 
    COALESCE(NEW.raw_user_meta_data->>'full_name', 'Người dùng mới'),
    NEW.raw_user_meta_data->>'avatar_url',
    'user'
  );
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE PROCEDURE public.handle_new_user();

-- 3. Bảng Categories
CREATE TABLE IF NOT EXISTS public.categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    type TEXT NOT NULL, -- article, music, script
    icon TEXT,
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'ict')
);

-- 4. Bảng Contents
CREATE TABLE IF NOT EXISTS public.contents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id UUID REFERENCES public.categories(id) ON DELETE SET NULL,
    title TEXT NOT NULL,
    slug TEXT UNIQUE,
    description TEXT,
    thumbnail_url TEXT,
    audio_url TEXT,
    content_type TEXT NOT NULL, -- article, music, script
    original_text TEXT,
    metadata JSONB,
    is_published BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'ict'),
    updated_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'ict')
);

-- 5. Bảng ContentLines
CREATE TABLE IF NOT EXISTS public.content_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    content_id UUID REFERENCES public.contents(id) ON DELETE CASCADE,
    start_time FLOAT NOT NULL,
    end_time FLOAT,
    text_jp TEXT NOT NULL,
    text_vi TEXT,
    order_index INT NOT NULL
);

-- 6. Bảng Flashcard Decks
CREATE TABLE IF NOT EXISTS public.flashcard_decks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES public.profiles(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'ict')
);

-- 7. Bảng Flashcards
CREATE TABLE IF NOT EXISTS public.flashcards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    deck_id UUID REFERENCES public.flashcard_decks(id) ON DELETE CASCADE,
    user_id UUID REFERENCES public.profiles(id) ON DELETE CASCADE,
    kanji TEXT,
    reading TEXT,
    meaning TEXT,
    sino_vietnamese TEXT,
    example_sentence TEXT,
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'ict')
);

-- 8. Bảng Dictionary Cache
CREATE TABLE IF NOT EXISTS public.dictionary_cache (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    base_form TEXT UNIQUE NOT NULL,
    reading TEXT,
    translation TEXT,
    sino_vietnamese TEXT,
    part_of_speech TEXT,
    created_at TIMESTAMPTZ DEFAULT (NOW() AT TIME ZONE 'ict')
);
