CREATE DATABASE IF NOT EXISTS rideshare_db;
USE rideshare_db;

-- 1. Kullanıcılar Tablosu
CREATE TABLE IF NOT EXISTS Kullanicilar (
    id INT AUTO_INCREMENT PRIMARY KEY,
    ad_soyad VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    sifre VARCHAR(255) NOT NULL,
    kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Araçlar Tablosu
CREATE TABLE IF NOT EXISTS Araclar (
    id INT AUTO_INCREMENT PRIMARY KEY,
    kullanici_id INT NOT NULL,
    marka VARCHAR(50) NOT NULL,
    model VARCHAR(50) NOT NULL,
    plaka VARCHAR(20) NOT NULL UNIQUE,
    renk VARCHAR(30),
    FOREIGN KEY (kullanici_id) REFERENCES Kullanicilar(id) ON DELETE CASCADE
);

-- 3. Yolculuklar Tablosu
CREATE TABLE IF NOT EXISTS Yolculuklar (
    id INT AUTO_INCREMENT PRIMARY KEY,
    kullanici_id INT NOT NULL,
    arac_id INT NOT NULL,
    kalkis_yeri VARCHAR(100) NOT NULL,
    varis_yeri VARCHAR(100) NOT NULL,
    tarih_saat DATETIME NOT NULL,
    fiyat DECIMAL(10,2) NOT NULL,
    koltuk_sayisi INT NOT NULL,
    FOREIGN KEY (kullanici_id) REFERENCES Kullanicilar(id) ON DELETE CASCADE,
    FOREIGN KEY (arac_id) REFERENCES Araclar(id) ON DELETE CASCADE
);

-- 4. Rezervasyonlar Tablosu
CREATE TABLE IF NOT EXISTS Rezervasyonlar (
    id INT AUTO_INCREMENT PRIMARY KEY,
    yolculuk_id INT NOT NULL,
    yolcu_id INT NOT NULL,
    koltuk_sayisi INT DEFAULT 1,
    rezervasyon_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (yolculuk_id) REFERENCES Yolculuklar(id) ON DELETE CASCADE,
    FOREIGN KEY (yolcu_id) REFERENCES Kullanicilar(id) ON DELETE CASCADE
);

-- ==================== PROSEDÜRLER ====================

DELIMITER //

-- Kullanıcı Giriş Kontrolü
CREATE PROCEDURE sp_LoginControl(IN p_email VARCHAR(100), IN p_sifre VARCHAR(255))
BEGIN
    SELECT id, ad_soyad FROM Kullanicilar WHERE email = p_email AND sifre = p_sifre;
END //

-- Araç Ekleme
CREATE PROCEDURE sp_InsertVehicle(IN p_kullanici_id INT, IN p_marka VARCHAR(50), IN p_model VARCHAR(50), IN p_plaka VARCHAR(20), IN p_renk VARCHAR(30))
BEGIN
    INSERT INTO Araclar(kullanici_id, marka, model, plaka, renk) VALUES (p_kullanici_id, p_marka, p_model, p_plaka, p_renk);
END //

-- Kullanıcı Araçlarını Listeleme
CREATE PROCEDURE sp_GetUserVehicles(IN p_kullanici_id INT)
BEGIN
    SELECT id, marka, model, plaka, renk FROM Araclar WHERE kullanici_id = p_kullanici_id;
END //

-- Yolculuk İlanı Oluşturma
CREATE PROCEDURE sp_InsertRide(IN p_kullanici_id INT, IN p_arac_id INT, IN p_kalkis_yeri VARCHAR(100), IN p_varis_yeri VARCHAR(100), IN p_tarih_saat DATETIME, IN p_fiyat DECIMAL(10,2), IN p_koltuk_sayisi INT)
BEGIN
    INSERT INTO Yolculuklar(kullanici_id, arac_id, kalkis_yeri, varis_yeri, tarih_saat, fiyat, koltuk_sayisi) VALUES (p_kullanici_id, p_arac_id, p_kalkis_yeri, p_varis_yeri, p_tarih_saat, p_fiyat, p_koltuk_sayisi);
END //

-- Tüm Yolculukları Detaylı Listeleme
CREATE PROCEDURE sp_GetAllRides()
BEGIN
    SELECT y.id, y.kullanici_id, k.ad_soyad AS surucu_adi, CONCAT(a.marka, ' ', a.model, ' (', a.plaka, ')') AS arac_bilgisi, y.kalkis_yeri, y.varis_yeri, y.tarih_saat, y.fiyat, y.koltuk_sayisi FROM Yolculuklar y INNER JOIN Kullanicilar k ON y.kullanici_id = k.id INNER JOIN Araclar a ON y.arac_id = a.id ORDER BY y.tarih_saat ASC;
END //

-- Akıllı Rezervasyon Yapma (Koltuk Düşürme Kurallı)
CREATE PROCEDURE sp_BookSeat(IN p_yolculuk_id INT, IN p_yolcu_id INT)
BEGIN
    DECLARE v_bos_koltuk INT;
    SELECT koltuk_sayisi INTO v_bos_koltuk FROM Yolculuklar WHERE id = p_yolculuk_id;
    IF v_bos_koltuk > 0 THEN
        INSERT INTO Rezervasyonlar(yolculuk_id, yolcu_id) VALUES (p_yolculuk_id, p_yolcu_id);
        UPDATE Yolculuklar SET koltuk_sayisi = koltuk_sayisi - 1 WHERE id = p_yolculuk_id;
    ELSE
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Bu yolculukta boş koltuk kalmamıştır!';
    END IF;
END //

-- Rezervasyon İptali
CREATE PROCEDURE sp_CancelBooking(IN p_rezervasyon_id INT)
BEGIN
    DECLARE v_yolculuk_id INT;
    SELECT yolculuk_id INTO v_yolculuk_id FROM Rezervasyonlar WHERE id = p_rezervasyon_id;
    DELETE FROM Rezervasyonlar WHERE id = p_rezervasyon_id;
    UPDATE Yolculuklar SET koltuk_sayisi = koltuk_sayisi + 1 WHERE id = v_yolculuk_id;
END //

-- Yolculuk İlanı İptali
CREATE PROCEDURE sp_CancelRide(IN p_yolculuk_id INT)
BEGIN
    DELETE FROM Yolculuklar WHERE id = p_yolculuk_id;
END //

DELIMITER ;