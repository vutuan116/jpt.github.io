﻿using JpT.Logic;
using JpT.Model;
using JpT.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JpT
{
    /// <summary>
    /// Interação lógica para SubWindowKanji.xam
    /// </summary>
    public partial class View_Kanji_Flashcard : UserControl
    {
        private ViewKanjiFlashcard _logic = new ViewKanjiFlashcard();
        private ViewKanjiFlashcardModel _model = new ViewKanjiFlashcardModel();
        private List<DataModel> dataOrigen = new List<DataModel>();
        private List<DataModel> dataTransform = new List<DataModel>();
        private int index = -1;
        private int countKotobaLastChange = 0;
        private TabName tabName = TabName.Kanji;
        private bool isShowAll = true;

        public View_Kanji_Flashcard(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = _model;
        }

        private void initCheckMemo()
        {
            tabSelectLesson.Visibility = Visibility.Visible;
            tabFlashCard.Visibility = Visibility.Hidden;
            tabShowAll.Visibility = Visibility.Hidden;
            tabUpdateHtml.Visibility = Visibility.Hidden;
            tbxKanji.Foreground = Brushes.Black;
            tbxHanviet.Foreground = Brushes.Black;
            tbxKotoba.Foreground = Brushes.Black;
            _model.LessonList = _logic.GetListLesson(tabName);
            cbxSelectChange(null, null);
        }

        private void createHtml(string folderPath)
        {
            ObservableCollection<LessonModel> lessonList = _logic.GetListLesson(TabName.Kanji);
            string listLessonKanji = "";
            foreach (LessonModel lesson in lessonList)
            {
                listLessonKanji += "<p><a href=\"JpT_Web\\layout\\" + lesson.LessonName + ".html\">" + lesson.LessonName + "</a></p>" + Environment.NewLine;
                List<DataModel> kanjiList = _logic.GetListObjByLesson(TabName.Kanji, lesson);
                string kanjiArr = "var kanjiArr = [";
                string hiraArr = "var hiraArr = [";
                string hanvietArr = "var hanvietArr = [";
                string meanArr = "var meanArr = [";
                foreach (DataModel kanji in kanjiList)
                {
                    kanjiArr += "\"" + kanji.Kanji.Trim() + "\",";
                    hiraArr += "\"" + kanji.Hiragana.Trim() + "\",";
                    hanvietArr += "\"" + kanji.CnVi.Trim().ToUpper() + "\",";
                    meanArr += "\"" + kanji.Mean.Trim().ToLower() + "\",";
                }
                kanjiArr = kanjiArr.Substring(0, kanjiArr.Length - 1) + "];";
                hiraArr = hiraArr.Substring(0, hiraArr.Length - 1) + "];";
                hanvietArr = hanvietArr.Substring(0, hanvietArr.Length - 1) + "];";
                meanArr = meanArr.Substring(0, meanArr.Length - 1) + "];";

                string content = readFile(Path.Combine(folderPath, "kanjiTemplate.html"));
                content = content.Replace("{Data}", kanjiArr + Environment.NewLine + hiraArr + Environment.NewLine + hanvietArr + Environment.NewLine + meanArr);
                writeFile(Path.Combine(folderPath, "layout", lesson.LessonName + ".html"), content);
            }

            lessonList = _logic.GetListLesson(TabName.Kotoba);
            string listLessonKotoba = "";
            foreach (LessonModel lesson in lessonList)
            {
                listLessonKotoba += "<p><a href=\"JpT_Web\\layout\\" + lesson.LessonName + ".html\">" + lesson.LessonName + "</a></p>" + Environment.NewLine;

                List<DataModel> kotobaList = _logic.GetListObjByLesson(TabName.Kotoba, lesson);
                string kanjiArr = "var kanjiArr = [";
                string hiraArr = "var hiraArr = [";
                string meanArr = "var meanArr = [";
                foreach (DataModel kanji in kotobaList)
                {
                    kanjiArr += "\"" + kanji.Kanji.Trim() + "\",";
                    hiraArr += "\"" + kanji.Hiragana.Trim() + "\",";
                    meanArr += "\"" + kanji.Mean.Trim().ToLower() + "\",";
                }
                kanjiArr = kanjiArr.Substring(0, kanjiArr.Length - 1) + "];";
                hiraArr = hiraArr.Substring(0, hiraArr.Length - 1) + "];";
                meanArr = meanArr.Substring(0, meanArr.Length - 1) + "];";

                string content = readFile(Path.Combine(folderPath, "kotobaTemplate.html"));
                content = content.Replace("{Data}", kanjiArr + Environment.NewLine + hiraArr + Environment.NewLine + meanArr);
                writeFile(Path.Combine(folderPath, "layout", lesson.LessonName + ".html"), content);
            }

            string contentIndex = readFile(Path.Combine(folderPath, "indexTemplate.html"));
            contentIndex = contentIndex.Replace("{List Kanji}", listLessonKanji);
            contentIndex = contentIndex.Replace("{List Từ Vựng}", listLessonKotoba);
            DirectoryInfo folderParent = Directory.GetParent(folderPath);
            writeFile(Path.Combine(folderParent.FullName, "index.html"), contentIndex);
        }

        string readFile(string path)
        {
            string content = File.ReadAllText(path);
            return content;
        }

        void writeFile(string path, string content)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var fileStream = File.Create(path);
            using (var sw = new StreamWriter(fileStream))
            {
                sw.WriteLine(content);
            }
        }

        private void checkkeydown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Right)
            {
                changeFlashCard();
            }
            else if (e.Key == System.Windows.Input.Key.Left)
            {
                isShowAll = true;
                backWord();
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                isShowAll = true;
                foreach (LessonModel lesson in _model.LessonList)
                {
                    lesson.IsSelected = false;
                }
                startBtnBackToMenu(null, null);
            }
            else if (e.Key == System.Windows.Input.Key.R)
            {
                _model.IsRepeat = !_model.IsRepeat;
            }
        }

        private void startBtnClick(object sender, RoutedEventArgs e)
        {
            dataOrigen.Clear();
            dataTransform.Clear();
            _model.Lesson = string.Empty;
            _model.Kanji = string.Empty;
            _model.Kotoba = string.Empty;
            _model.HanViet = string.Empty;
            _model.Hiragana = string.Empty;
            _model.Meaning = string.Empty;
            countKotobaLastChange = 0;
            index = -1;
            isShowAll = true;

            foreach (LessonModel lesson in _model.LessonList)
            {
                if (lesson.IsSelected)
                {
                    dataOrigen.AddRange(_logic.GetListObjByLesson(tabName, lesson));
                }
            }

            dataTransform.AddRange(dataOrigen);
            if (dataOrigen.Count == 0)
            {
                MessageBox.Show("Không có bài học nào được chọn hoặc không có từ vựng phù hợp.");
            }
            else
            {
                tabSelectLesson.Visibility = Visibility.Hidden;
                tabFlashCard.Visibility = Visibility.Visible;
                tabShowAll.Visibility = Visibility.Hidden;
                processbar.Maximum = dataOrigen.Count;
                changeFlashCard();
                changeColorText();
                tbxKanji.Focus();
            }
        }

        private void changeFlashCard()
        {
            isShowAll = !isShowAll;

            if (index == dataTransform.Count - 1 && ((!string.IsNullOrEmpty(_model.Meaning) && tabName == TabName.Kanji) || (!string.IsNullOrEmpty(_model.Hiragana) && tabName == TabName.Kotoba)))
            {
                if (randomFlashcard())
                {
                    return;
                }
            }

            if (((!string.IsNullOrEmpty(_model.Meaning) || string.IsNullOrEmpty(_model.Kanji)) && tabName == TabName.Kanji) || ((!string.IsNullOrEmpty(_model.Hiragana) || string.IsNullOrEmpty(_model.Kotoba)) && tabName == TabName.Kotoba))
            {
                index++;
            }

            _model.Lesson = dataTransform[index].Lesson;
            _model.IsRepeat = dataTransform[index].IsRepeat;
            processbar.Value = index - countKotobaLastChange + 1;
            if (tabName == TabName.Kanji)
            {
                _model.Kotoba = string.Empty;
                if (isShowAll)
                {
                    _model.Kanji = dataTransform[index].Kanji;
                    _model.HanViet = dataTransform[index].CnVi;
                    _model.Hiragana = dataTransform[index].Hiragana;
                    _model.Meaning = dataTransform[index].Mean;
                }
                else
                {
                    _model.Kanji = dataTransform[index].Kanji;
                    _model.HanViet = string.Empty;
                    _model.Hiragana = string.Empty;
                    _model.Meaning = string.Empty;
                }
            }
            else
            {
                _model.Kanji = string.Empty;
                _model.Meaning = string.Empty;
                if (isShowAll)
                {
                    _model.Kotoba = dataTransform[index].Kanji;
                    _model.HanViet = dataTransform[index].Hiragana;
                    _model.Hiragana = dataTransform[index].Mean;
                }
                else
                {
                    _model.Kotoba = dataTransform[index].Kanji;
                    _model.HanViet = dataTransform[index].Hiragana;
                    _model.Hiragana = string.Empty;
                }
            }
        }

        private void backWord()
        {
            if (index == 0)
            {
                return;
            }
            index--;
            if (tabName == TabName.Kanji)
            {
                _model.Kanji = dataTransform[index].Kanji;
                _model.Kotoba = string.Empty;
                _model.HanViet = dataTransform[index].CnVi;
                _model.Hiragana = dataTransform[index].Hiragana;
                _model.Meaning = dataTransform[index].Mean;
            }
            else
            {
                _model.Kanji = string.Empty;
                _model.Kotoba = dataTransform[index].Kanji;
                _model.HanViet = dataTransform[index].Hiragana;
                _model.Hiragana = dataTransform[index].Mean;
                _model.Meaning = string.Empty;
            }
            _model.Lesson = dataTransform[index].Lesson;
            _model.IsRepeat = dataTransform[index].IsRepeat;
            processbar.Value = index - countKotobaLastChange + 1;
        }

        private void changeColorText()
        {
            if (_model.IsRepeat)
            {
                if (tabName == TabName.Kanji)
                {
                    tbxKanji.Foreground = Brushes.Red;
                }
                else
                {
                    tbxHanviet.Foreground = Brushes.Red;
                    tbxKotoba.Foreground = Brushes.Red;
                }
            }
            else
            {
                if (tabName == TabName.Kanji)
                {
                    tbxKanji.Foreground = Brushes.Black;
                }
                else
                {
                    tbxHanviet.Foreground = Brushes.Black;
                    tbxKotoba.Foreground = Brushes.Black;
                }
            }
        }

        private bool randomFlashcard()
        {
            List<DataModel> temp = new List<DataModel>(dataOrigen);

            int i = 0;
            while (i < temp.Count)
            {
                if (temp[i].IsRepeat)
                {
                    i++;
                }
                else
                {
                    temp.RemoveAt(i);
                }
            }
            if (temp.Count == 0)
            {
                //_logic.UpdateLastLearning(tabName, _model.LessonList);

                MessageBoxResult result = MessageBox.Show("Đã hoàn thành bài học ngày hôm nay." + Environment.NewLine +
                    "Bạn có muốn học lại không?", "Thông báo", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    startBtnClick(null, null);
                    return true;
                }
                else
                {
                    startBtnBackToMenu(null, null);
                    return true;
                }
            }

            Random random = new Random();
            processbar.Maximum = temp.Count;
            countKotobaLastChange = dataTransform.Count;
            while (temp.Count != 0)
            {
                i = random.Next(0, temp.Count - 1);
                dataTransform.Add(temp[i]);
                temp.RemoveAt(i);
            }
            return false;
        }

        private void startBtnShowAll(object sender, RoutedEventArgs e)
        {
            List<DataModel> result = new List<DataModel>();
            foreach (LessonModel lesson in _model.LessonList)
            {
                if (lesson.IsSelected)
                {
                    result.AddRange(_logic.GetListObjByLesson(tabName, lesson, true, true));
                }
            }

            if (result.Count == 0)
            {
                MessageBox.Show("Không có bài học nào được chọn hoặc không có từ vựng phù hợp.");
                return;
            }

            tabSelectLesson.Visibility = Visibility.Hidden;
            tabFlashCard.Visibility = Visibility.Hidden;
            tabShowAll.Visibility = Visibility.Visible;
            btnBackMenu.Focus();

            _model.KanjiList = new ObservableCollection<DataModel>(result);
        }

        private void startBtnBackToMenu(object sender, RoutedEventArgs e)
        {
            _logic.SaveListHard(tabName, _model.KanjiList);

            initCheckMemo();
        }

        private void startBtnLearningHard(object sender, RoutedEventArgs e)
        {
            dataOrigen.Clear();
            dataTransform.Clear();
            _model.Lesson = string.Empty;
            _model.Kanji = string.Empty;
            _model.Kotoba = string.Empty;
            _model.HanViet = string.Empty;
            _model.Hiragana = string.Empty;
            _model.Meaning = string.Empty;

            index = -1;
            foreach (LessonModel lesson in _model.LessonList)
            {
                if (lesson.IsSelected)
                {
                    dataOrigen.AddRange(_logic.GetListObjByLesson(tabName, lesson, true));
                }
            }
            dataTransform.AddRange(dataOrigen);
            if (dataOrigen.Count == 0)
            {
                MessageBox.Show("Không có bài học nào được chọn hoặc không có từ vựng phù hợp.");
            }
            else
            {
                tabSelectLesson.Visibility = Visibility.Hidden;
                tabFlashCard.Visibility = Visibility.Visible;
                tabShowAll.Visibility = Visibility.Hidden;
                processbar.Maximum = dataOrigen.Count;
                changeFlashCard();
                tbxKanji.Focus();
            }
        }

        private void cbxRepeatChange(object sender, RoutedEventArgs e)
        {
            if (index != -1 && dataTransform.Count != 0)
            {
                dataTransform[index].IsRepeat = _model.IsRepeat;
            }
            changeColorText();
            tbxKanji.Focus();
        }

        private void TabControl_SelectionChanged(object sender, object e)
        {
            TabItem tab = (TabItem)tabControl.SelectedItem;

            if (tab.Name.Equals("tabKanji"))
            {
                tabName = TabName.Kanji;
            }
            else
            {
                tabName = TabName.Kotoba;
            }
            initCheckMemo();
        }

        private void Btn_UpdateHtml_Click(object sender, RoutedEventArgs e)
        {
            string folderExe = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = folderExe.IndexOf(@"\jp\") + 4;
            string pathFolderData = Path.Combine(folderExe.Substring(0, index), @"JpT_Web\data\data.js");
            txt_Folder.Text = pathFolderData;

            tabUpdateHtml.Visibility = Visibility.Visible;
        }

        public void Btn_Run_Update_Html_Click(object sender, RoutedEventArgs e)
        {
            updateFileDataJs(txt_Folder.Text);
            MessageBox.Show("Đã hoàn thành update file data tại:" + Environment.NewLine + txt_Folder.Text);
            tabUpdateHtml.Visibility = Visibility.Hidden;
        }

        private void updateFileDataJs(string fileData)
        {
            List<LessonModel> lessonList;
            lessonList = _logic.GetListLesson(TabName.Kanji).ToList();
            lessonList.AddRange(_logic.GetListLesson(TabName.Kotoba));

            StringBuilder sb = new StringBuilder("var data = '[");

            foreach (LessonModel lesson in lessonList)
            {
                List<DataModel> dataList = _logic.GetListObjByLesson(lesson.Type.Equals("KJ") ? TabName.Kanji : TabName.Kotoba, lesson);
                if (dataList.Count == 0)
                    continue;

                string kanji = "";
                string hiragana = "";
                string mean = "";
                foreach (DataModel data in dataList)
                {
                    kanji += string.IsNullOrEmpty(kanji) ? ("\"" + data.Kanji + "\"") : (",\"" + data.Kanji + "\"");
                    hiragana += string.IsNullOrEmpty(hiragana) ? ("\"" + data.Hiragana + "\"") : (",\"" + data.Hiragana + "\"");
                    mean += string.IsNullOrEmpty(mean) ? ("\"" + data.Mean + "\"") : (",\"" + data.Mean + "\"");
                }
                if (sb.Length > 20)
                {
                    sb.Append(",");
                }

                sb.Append("{\"level\": \"" + dataList[0].Level + "\",\"type\": \"" + dataList[0].Type + "\",\"lesson\": \"" + lesson.LessonName + "\",\"kanji\":[" + kanji + "],\"hira\":[" + hiragana + "],\"mean\":[" + mean + "]}");
            }
            sb.Append("]';");
            string content = sb.ToString();
            writeFile(txt_Folder.Text, content);
        }

        private void UnSelectClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            foreach (LessonModel lesson in _model.LessonList)
            {
                lesson.IsSelected = false;
            }
            _model.ContentSelected = "Selected: 0";
        }

        private void cbxSelectChange(object sender, RoutedEventArgs e)
        {
            int count = 0;
            string selected = string.Empty;
            foreach (LessonModel lesson in _model.LessonList)
            {
                if (lesson.IsSelected)
                {
                    count++;
                    selected += Environment.NewLine + " - " + lesson.LessonName;
                }
            }
            _model.ContentSelected = "Selected: " + count + selected;
        }

        private void hiddenUpdateHtml(object sender, RoutedEventArgs e)
        {
            tabUpdateHtml.Visibility = Visibility.Hidden;
        }
    }
}
